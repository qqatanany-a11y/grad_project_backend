

using events.domain.Entites;

namespace events.domain.Entities
{
    public class Venue : BaseEntity
    {
        public string Name { get; private set; } 
        public string Description { get; private set; }
        public string City { get; private set; }
        public string Address { get; private set; }
        public int Capacity { get; private set; }
        public decimal MinimalPrice { get; private set; }
        public bool IsActive { get; private set; } = true;
        public int CompanyId { get; private set; }
        public Company Company { get; private set; }


        public Venue(string name, string description, string city,
             string address, int capacity, decimal minimalPrice, int companyId)
        {
            Name = name;
            Description = description;
            City = city;
            Address = address;
            Capacity = capacity;
            MinimalPrice = minimalPrice;
            CompanyId = companyId;
            IsActive = true;
        }

        public void Update(string name, string description, string city,
                           string address, int capacity, decimal minimalPrice, bool isActive)
        {
            Name = name;
            Description = description;
            City = city;
            Address = address;
            Capacity = capacity;
            MinimalPrice = minimalPrice;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }


        public List<VenueImage> Images { get; private set; } = new List<VenueImage>();
        public List<VenueEventType> VenueEventTypes { get; private set; } = new List<VenueEventType>();
        public List<VenueAvailability> Availabilities { get; private set; } = new List<VenueAvailability>();
        public List<Booking> Bookings { get; private set; } = new List<Booking>();
        public List<Review> Reviews { get; private set; } = new List<Review>();
    }
}
