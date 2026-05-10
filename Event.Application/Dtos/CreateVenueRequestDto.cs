using events.domain.Entities;

namespace Event.Application.Dtos
{
    public class CreateVenueRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public VenueType Type { get; set; }
        public VenueCategory Category { get; set; }
        public PricingType PricingType { get; set; }
        public decimal? PricePerHour { get; set; }
        public decimal DepositPercentage { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public List<VenueTimeSlotUpsertDto>? TimeSlots { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
