namespace Event.Application.Dtos
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal ServicesPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal DepositPercentage { get; set; }
        public decimal DepositAmount { get; set; }
        public bool CanPay { get; set; }
        public BookingPaymentDto? Payment { get; set; }
        public string? BrideIdDocumentDataUrl { get; set; }
        public string? BridegroomIdDocumentDataUrl { get; set; }
        public List<SelectedServiceResponseDto> Services { get; set; } = new();
    }
}
