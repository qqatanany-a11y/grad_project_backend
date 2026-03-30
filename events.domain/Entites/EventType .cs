namespace events.domain.Entities
{
    public class EventType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<VenueEventType> VenueEventTypes { get; set; } = new List<VenueEventType>();
    }
}
