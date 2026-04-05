using Event.Application.Dtos;

namespace Event.Application.Services
{
    public interface IClientAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}