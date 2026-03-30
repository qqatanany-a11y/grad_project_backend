using events.domain.Entites;

namespace events.domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid BookingId { get; private set; }    
        public decimal Amount { get; private set; }
        public string? PaymentMethod { get; private set; }
        public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
        public DateTime? PaidAt { get; private set; }
        public Booking Booking { get; private set; } = null!;
    }
}
