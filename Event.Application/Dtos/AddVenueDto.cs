using events.domain.Entities;

namespace Event.Application.Dtos
{
    public class AddVenueDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public int Capacity { get; set; }

        public VenueType Type { get; set; }

        public PricingType PricingType { get; set; }
        public decimal? PricePerHour { get; set; }

        public decimal DepositPercentage { get; set; }

        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? WebsiteUrl { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}