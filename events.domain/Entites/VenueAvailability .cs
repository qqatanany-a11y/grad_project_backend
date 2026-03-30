using events.domain.Entites;

namespace events.domain.Entities
{
    public class VenueAvailability : BaseEntity
    {
        public Guid VenueId { get; private set; }        
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public bool IsAvailable { get; private set; } = true;
        public Venue Venue { get; private set; } = null!;
    }
}
