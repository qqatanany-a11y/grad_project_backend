namespace Event.Application.Dtos
{
    public class VenueEditRequestDataDto
    {
        public int VenueId { get; set; }
        public string CompanyName { get; set; }
        public VenueEditRequestDto Current { get; set; }
        public VenueEditRequestDto Requested { get; set; }
    }
}
