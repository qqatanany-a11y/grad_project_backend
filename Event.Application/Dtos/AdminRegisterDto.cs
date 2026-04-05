// AdminRegisterDto.cs
namespace Event.Application.Dtos
{
    public class AdminRegisterDto
    {
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; } 
        public string Email { get; set; }
        public string PhoneNumber { get; set; } 
        public string Password { get; set; } 
    }
}

// AdminLoginDto.cs
namespace Event.Application.Dtos
{
    public class AdminLoginDto
    {
        public string Email { get; set; } 
        public string Password { get; set; } 
    }
}

// AdminResponseDto.cs
namespace Event.Application.Dtos
{
    public class AdminResponseDto
    {
        public string Token { get; set; } 
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } = "SystemAdmin";
    }
}