using events.domain.Entites;

namespace events.domain.Entities
{
    public class VenueImage : BaseEntity
    {
        public int VenueId { get; private set; }         
        public string ImageUrl { get; private set; } = null!;
        public bool IsCover { get; private set; } = false;
        public Venue Venue { get; private set; } = null!;
    }
}
