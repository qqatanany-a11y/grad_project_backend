namespace events.domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }    
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public virtual Booking Booking { get; set; } = null!;
    }
}
