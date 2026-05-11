using events.domain.Entities;

namespace Event.Application.Dtos
{
    public class VenueDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public VenueType Type { get; set; }
        public VenueCategory Category { get; set; }
        public PricingType PricingType { get; set; }
        public decimal? PricePerHour { get; set; }
        public decimal DepositPercentage { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public double AverageRating { get; set; }
        public List<VenueTimeSlotDto> TimeSlots { get; set; } = new();
    }
}
