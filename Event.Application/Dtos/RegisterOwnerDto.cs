namespace Event.Application.Dtos
{
    public class RegisterOwnerDto
    {
        public string FirstName { get; set; } 
        public string MiddleName { get; set; }
        public string LastName { get; set; } 
        public string Email { get; set; } 
        public string PhoneNumber { get; set; } 
        public string Password { get; set; } 
        public string CompanyName { get; set; } 
        public string BusinessPhone { get; set; }
        public string BusinessAddress { get; set; }
        public int CompanyId { get; set; }

    }
}