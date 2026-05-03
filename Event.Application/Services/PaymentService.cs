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

        public PaymentService(IPaymentRepo paymentRepo, IBookingRepo bookingRepo)
        {
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<string> PayAsync(int userId, PayBookingDto dto)
        {
            var booking = await _bookingRepo.GetByIdAsync(dto.BookingId);

            if (booking == null)
                throw new Exception("Booking not found");

            if (booking.UserId != userId)
                throw new Exception("Not allowed");

            if (booking.Status != BookingStatusEnum.Pending)
                throw new Exception("Only pending bookings can be paid.");

            if (dto.PaymentMethod != PaymentMethodEnum.Cash &&
                dto.PaymentMethod != PaymentMethodEnum.CliQ)
                throw new Exception("Invalid payment method.");

            var payment = await _paymentRepo.GetByBookingIdAsync(dto.BookingId);

            if (payment == null)
            {
                var depositAmount = booking.TotalPrice * (booking.Venue.DepositPercentage / 100);

                payment = new Payment(
                    booking.Id,
                    depositAmount,
                    dto.PaymentMethod
                );

                await _paymentRepo.AddAsync(payment);
            }

            if (payment.Status == PaymentStatus.Paid)
                throw new Exception("Payment already paid.");

            payment.MarkAsPaid(dto.PaymentMethod);

            await _paymentRepo.SaveChangesAsync();

            return "Payment completed successfully.";
        }
    }
}