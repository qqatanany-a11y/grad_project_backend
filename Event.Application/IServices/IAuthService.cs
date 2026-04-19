using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAdminAsync(RegisterDto dto);

        Task<AuthResponseDto> RegisterOwnerAsync(RegisterOwnerDto dto);
        Task CreateOwnerRequestAsync(RegisterOwnerDto dto);
    }
}