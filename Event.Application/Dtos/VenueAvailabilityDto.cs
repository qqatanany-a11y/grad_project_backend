namespace Event.Application.Dtos
{
    public class VenueAvailabilityDto
    {
        public int Id { get; set; }
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}