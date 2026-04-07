using System.ComponentModel.DataAnnotations;
namespace Event.Application.Dtos
{
    public class RegisterOwnerDto
    {
        [Required] public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        [Required] public string LastName { get; set; }
        [Required][EmailAddress] public string Email { get; set; }
        [Required] public string PhoneNumber { get; set; }
        [Required][MinLength(8)] public string Password { get; set; }

        // بيانات الشركة
        [Required] public string CompanyName { get; set; }
        [Required] public string BusinessPhone { get; set; }
        [Required] public string BusinessAddress { get; set; }
        // ← شلنا CompanyId لأن إحنا منولدوا، مش الـ User
    }
}