namespace events.domain.Entities
{
    public class VenueEventType
    {
        public int VenueId { get; set; }        
        public int EventTypeId { get; set; }     

        
        public virtual Venue Venue { get; set; } = null!;
     
        public virtual EventType EventType { get; set; } = null!;
    }
}
