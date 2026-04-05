using Event.Application.Dtos;

namespace Event.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterOwnerAsync(RegisterOwnerDto dto);

    }
}