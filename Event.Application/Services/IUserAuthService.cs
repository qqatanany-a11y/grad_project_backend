using Event.Application.Dtos;

namespace Event.Application.Services
{
    public interface IUserAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}