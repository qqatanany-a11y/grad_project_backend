namespace Event.Application.Dtos
{
    public class VenueAvailabilityItemDto
    {
        public int Id { get; set; }
        public int VenueId { get; set; }
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public bool IsBooked { get; set; }
    }
}