namespace Event.Application.Dtos
{
    public class AddVenueServiceOptionDto
    {
        public int VenueId { get; set; }
        public int ServiceId { get; set; }
        public decimal Price { get; set; }
    }
}