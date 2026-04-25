namespace Event.Application.Dtos
{
    public class OwnerRequestDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string BusinessAddress { get; set; }
        public string BusinessPhone { get; set; }
        public string VenueName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
