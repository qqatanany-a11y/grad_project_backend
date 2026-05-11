using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepo _paymentRepo;
        private readonly IBookingRepo _bookingRepo;
        private readonly IEmailService _emailService;

        public PaymentService(IPaymentRepo paymentRepo, IBookingRepo bookingRepo, IEmailService emailService)
        {
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
            _emailService = emailService;
        }

        public async Task<string> PayAsync(int userId, PayBookingDto dto)
        {
            var booking = await _bookingRepo.GetByIdAsync(dto.BookingId);

            if (booking == null)
                throw new Exception("Booking not found");

            if (booking.UserId != userId)
                throw new Exception("Not allowed");

            if (booking.Status != BookingStatusEnum.Confirmed)
                throw new Exception("Only confirmed bookings can be paid.");

            if (dto.PaymentMethod != PaymentMethodEnum.Cash &&
                dto.PaymentMethod != PaymentMethodEnum.CliQ)
                throw new Exception("Invalid payment method.");

            if (dto.PaymentMethod == PaymentMethodEnum.CliQ &&
                string.IsNullOrWhiteSpace(dto.CliqTransferImageDataUrl))
                throw new Exception("CliQ transfer image is required.");

            var payment = await _paymentRepo.GetByBookingIdAsync(dto.BookingId);
            var depositAmount = payment?.Amount
                ?? CalculateDepositAmount(booking.TotalPrice, booking.Venue.DepositPercentage);

            // Deposit is always required — minimum fallback is 10% of total price

            if (payment == null)
            {
                payment = new Payment(
                    booking.Id,
                    depositAmount,
                    dto.PaymentMethod
                );

                await _paymentRepo.AddAsync(payment);
                booking.SetPayment(payment);
            }

            if (payment.Status == PaymentStatus.Paid)
                throw new Exception("Payment already paid.");

            string successMessage;

            if (dto.PaymentMethod == PaymentMethodEnum.Cash)
            {
                payment.SelectCashPayment();
                successMessage = "Cash payment option saved successfully.";
            }
            else
            {
                payment.MarkAsPaid(dto.PaymentMethod, dto.CliqTransferImageDataUrl);
                successMessage = "Payment completed successfully.";
            }

            await _paymentRepo.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                booking.User.Email,
                "Your payment has been updated",
                $@"
<h2>Your payment has been updated</h2>
<p>Venue: {booking.Venue.Name}</p>
<p>Date: {booking.BookingDate:yyyy-MM-dd}</p>
<p>Payment method: {dto.PaymentMethod}</p>
<p>Deposit amount: {depositAmount:0.00} JOD</p>
<p>Payment status: {payment.Status}</p>");

            return successMessage;
        }

        private static decimal CalculateDepositAmount(decimal totalPrice, decimal depositPercentage)
        {
            // Always apply a minimum of 10% if no deposit percentage is configured
            var effectivePct = depositPercentage > 0 ? depositPercentage : 10m;
            return decimal.Round(totalPrice * (effectivePct / 100m), 2, MidpointRounding.AwayFromZero);
        }
    }
}
