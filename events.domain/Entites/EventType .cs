using events.domain.Entites;

namespace events.domain.Entities
{
    public class EventType : BaseEntity
    {
        public string Name { get;  set; }
        public DateTime CreatedAt { get;  set; } = DateTime.UtcNow;

        public List<VenueEventType> VenueEventTypes { get;  set; } = new List<VenueEventType>();
    }
}
