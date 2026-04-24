using events.domain.Entites;

namespace events.domain.Entities
{
    public class VenueServiceOption : BaseEntity
    {
        public int VenueId { get; private set; }
        public Venue Venue { get; private set; } = null!;

        public int ServiceId { get; private set; }
        public Service Service { get; private set; } = null!;

        public decimal Price { get; private set; }
        public bool IsActive { get; private set; } = true;

        private VenueServiceOption() { }

        public VenueServiceOption(int venueId, int serviceId, decimal price)
        {
            VenueId = venueId;
            ServiceId = serviceId;
            Price = price;
            IsActive = true;
        }

        public void UpdatePrice(decimal price)
        {
            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}