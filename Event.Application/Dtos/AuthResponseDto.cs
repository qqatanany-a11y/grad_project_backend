namespace Event.Application.Dtos
{
    public class AuthResponseDto
    {
        public string? Token { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public bool IsFirstLogin { get; set; }
        public bool RequiresPasswordChange { get; set; }
        public bool IsOwner { get; set; }
    }
}
