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

        public string? FacebookUrl { get; private set; }
        public string? InstagramUrl { get; private set; }
        public string? WebsiteUrl { get; private set; }

        public VenueType Type { get; private set; }

        public decimal DepositPercentage { get; private set; }

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
            VenueType type,
            PricingType pricingType,
            decimal? pricePerHour,
            decimal depositPercentage,
            string? facebookUrl,
            string? instagramUrl,
            string? websiteUrl
            )
        {
            Name = name;
            Description = description;
            City = city;
            Address = address;
            Capacity = capacity;
            CompanyId = companyId;
            Type = type;
            PricingType = pricingType;
            PricePerHour = pricingType == PricingType.Hourly ? pricePerHour : null;
            DepositPercentage = depositPercentage;
            IsActive = true;
            FacebookUrl = facebookUrl;
            InstagramUrl = instagramUrl;
            WebsiteUrl = websiteUrl;
        }

        public void Update(
            string name,
            string description,
            string city,
            string address,
            int capacity,
            bool isActive,
            VenueType type,
            PricingType pricingType,
            decimal? pricePerHour,
            decimal depositPercentage,
            string? facebookUrl,
            string? instagramUrl,
            string? websiteUrl
            )
        {
            Name = name;
            Description = description;
            City = city;
            Address = address;
            Capacity = capacity;
            IsActive = isActive;
            Type = type;
            PricingType = pricingType;
            PricePerHour = pricingType == PricingType.Hourly ? pricePerHour : null;
            DepositPercentage = depositPercentage;
            FacebookUrl = facebookUrl;
            InstagramUrl = instagramUrl;
            WebsiteUrl = websiteUrl;

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