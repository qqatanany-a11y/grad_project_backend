using events.domain.Entites;

namespace events.domain.Entities
{
    public class VenueEventType : BaseEntity
    {
        public Guid VenueId { get; private set; }        
        public int EventTypeId { get; private set; }     

        public  Venue Venue { get; private set; } = null!;
     
        public  EventType EventType { get; private set; } = null!;
    }
}
