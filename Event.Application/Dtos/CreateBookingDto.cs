namespace Event.Application.Dtos
{
    public class CreateBookingDto
    {
        public int VenueId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int GuestsCount { get; set; }

        public int? VenueAvailabilityId { get; set; }
        public List<int> VenueServiceOptionIds { get; set; } = new();
        public string? BrideIdDocumentDataUrl { get; set; }
        public string? BridegroomIdDocumentDataUrl { get; set; }
    }
}
