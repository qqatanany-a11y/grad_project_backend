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
        public bool IsActive { get; private set; } = true;

        public int CompanyId { get; private set; }
        public Company Company { get; private set; } = null!;

        public PricingType PricingType { get; private set; }
        public decimal? PricePerHour { get; private set; }

        public List<VenueImage> Images { get; private set; } = new();
        public List<VenueEventType> VenueEventTypes { get; private set; } = new();
        public List<VenueAvailability> Availabilities { get; private set; } = new();
        public List<Booking> Bookings { get; private set; } = new();
        public List<Review> Reviews { get; private set; } = new();
        public List<VenueServiceOption> VenueServices { get; private set; } = new();

        private Venue() { }

        public Venue(
            string name,
            string description,
            string city,
            string address,
            int capacity,
            int companyId,
            PricingType pricingType,
            decimal? pricePerHour)
        {
            Name = name;
            Description = description;
            City = city;
            Address = address;
            Capacity = capacity;
            CompanyId = companyId;
            PricingType = pricingType;
            PricePerHour = pricingType == PricingType.Hourly ? pricePerHour : null;
            IsActive = true;
        }

        public void Update(
            string name,
            string description,
            string city,
            string address,
            int capacity,
            bool isActive,
            PricingType pricingType,
            decimal? pricePerHour)
        {
            Name = name;
            Description = description;
            City = city;
            Address = address;
            Capacity = capacity;
            IsActive = isActive;
            PricingType = pricingType;
            PricePerHour = pricingType == PricingType.Hourly ? pricePerHour : null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddImages(List<string> imageUrls)
        {
            if (imageUrls == null || imageUrls.Count < 10)
                throw new Exception("Minimum 10 images are required for a venue.");

            Images.Clear();

            for (int i = 0; i < imageUrls.Count; i++)
            {
                Images.Add(new VenueImage(imageUrls[i], i == 0));
            }
        }
    }
}