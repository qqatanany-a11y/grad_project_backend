using events.domain.Entites;

namespace events.domain.Entities
{
    // sport events, music events, corporate events, private events, etc.
    public class EventType : BaseEntity
    {
        public string Name { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public List<VenueEventType> VenueEventTypes { get; private set; } = new List<VenueEventType>();
    }
}
