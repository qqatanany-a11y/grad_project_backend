namespace Event.Application.Dtos
{
    public class CreateVenueRequestDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public int Capacity { get; set; }
        public VenueCategory Category { get; set; }
        public PricingType PricingType { get; set; }
        public decimal? PricePerHour { get; set; }
        public List<VenueTimeSlotUpsertDto>? TimeSlots { get; set; }
    }
}
