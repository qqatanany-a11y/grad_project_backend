namespace events.domain.Entities
{
    public class VenueAvailability
    {
        public int Id { get; set; }
        public int VenueId { get; set; }        
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public virtual Venue Venue { get; set; } = null!;
    }
}
