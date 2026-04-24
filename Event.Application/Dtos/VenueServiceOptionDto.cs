namespace Event.Application.Dtos
{
    public class VenueServiceOptionDto
    {
        public int Id { get; set; }
        public int VenueId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}