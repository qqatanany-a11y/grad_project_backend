using Event.Application.Dtos;

namespace Event.Application.Services
{
    public interface IAdminAuthService
    {
        Task<AdminResponseDto> RegisterAsync(AdminRegisterDto dto);
        Task<AdminResponseDto> LoginAsync(AdminLoginDto dto);
    }
}