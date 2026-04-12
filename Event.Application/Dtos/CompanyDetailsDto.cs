namespace Event.Application.Dtos
{
    public class CompanyDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public List<VenueDto> Venues { get; set; }
    }
}