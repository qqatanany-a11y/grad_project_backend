namespace Event.Application.Dtos
{
    public class VenueEditRequestDataDto
    {
        public int VenueId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public VenueEditRequestDto Current { get; set; } = new();
        public VenueEditRequestDto Requested { get; set; } = new();
    }
}
