

namespace Event.Application.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } 
        public string FullName { get; set; } 
        public string Email { get; set; }
        public string Role { get; set; }
        public int? CompanyId { get; set; } 
        public bool IsFirstLogin { get; set; }
        public bool IsOwner { get; set; } = false;
    }
}
