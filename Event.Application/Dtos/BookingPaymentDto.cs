namespace Event.Application.Dtos
{
    public class BookingPaymentDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public string? CliqTransferImageDataUrl { get; set; }
    }
}
