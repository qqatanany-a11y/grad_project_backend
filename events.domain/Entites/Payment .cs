using events.domain.Entites;

namespace events.domain.Entities
{
    public class Payment : BaseEntity
    {
        private Payment() { }

        public int BookingId { get; private set; }
        public Booking Booking { get; private set; } = null!;

        public decimal Amount { get; private set; }
        public PaymentMethodEnum PaymentMethod { get; private set; }
        public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
        public DateTime? PaidAt { get; private set; }
        public string? CliqTransferImageDataUrl { get; private set; }

        public Payment(int bookingId, decimal amount, PaymentMethodEnum paymentMethod)
        {
            BookingId = bookingId;
            Amount = amount;
            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Pending;
        }

        public void SelectCashPayment()
        {
            PaymentMethod = PaymentMethodEnum.Cash;
            Status = PaymentStatus.Pending;
            PaidAt = null;
            CliqTransferImageDataUrl = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsPaid(PaymentMethodEnum paymentMethod, string? cliqTransferImageDataUrl = null)
        {
            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Paid;
            PaidAt = DateTime.UtcNow;
            CliqTransferImageDataUrl = paymentMethod == PaymentMethodEnum.CliQ
                ? cliqTransferImageDataUrl
                : null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
