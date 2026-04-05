using events.domain.Entites;

namespace events.domain.Entities
{
    public class VenueEventType : BaseEntity
    {
        public int CompanyId { get; private set; }
        public Company Company { get; private set; }

        public int VenueId { get; private set; }        
        public int EventTypeId { get; private set; }     

        public  Venue Venue { get; private set; } 
     
        public  EventType EventType { get; private set; }  
    }
}
