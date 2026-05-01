using events.domain.Entities;

namespace Event.Application.Dtos
{
    public class VenueDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }

        public string CompanyName { get; set; }
        public int CompanyId { get; set; }

        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public VenueType Type { get; set; }

        public PricingType PricingType { get; set; }
        public decimal? PricePerHour { get; set; }

        public decimal DepositPercentage { get; set; }
    }
}