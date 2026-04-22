namespace Event.Application.Dtos
{
    public class CreateBookingResponseDto
    {
        public int BookingId { get; set; }
        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal ServicesPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<SelectedServiceResponseDto> Services { get; set; } = new();
    }

    public class SelectedServiceResponseDto
    {
        public int VenueServiceOptionId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}