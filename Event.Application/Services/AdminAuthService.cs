using Event.Application.Dtos;
using events.domain.Entities;
using events.domain.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Event.Application.Services
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly ISystemAdminRepo _adminRepo;
        private readonly IConfiguration _config;

        public AdminAuthService(ISystemAdminRepo adminRepo, IConfiguration config)
        {
            _adminRepo = adminRepo;
            _config = config;
        }

        public async Task<AdminResponseDto> RegisterAsync(AdminRegisterDto dto)
        {
            var existing = await _adminRepo.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("البريد الإلكتروني مسجل مسبقاً");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var admin = new SystemAdmin(
                dto.Email,
                passwordHash,
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                dto.MiddleName
            );

            await _adminRepo.AddAsync(admin);
            var token = GenerateJwtToken(admin);

            return new AdminResponseDto
            {
                Token = token,
                FullName = admin.FullName,
                Email = admin.Email,
                Role = "SystemAdmin"
            };
        }

        public async Task<AdminResponseDto> LoginAsync(AdminLoginDto dto)
        {
            var admin = await _adminRepo.GetByEmailAsync(dto.Email);
            if (admin == null)
                throw new Exception("البريد الإلكتروني أو كلمة المرور غلط");

            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash);
            if (!isValid)
                throw new Exception("البريد الإلكتروني أو كلمة المرور غلط");

            var token = GenerateJwtToken(admin);

            return new AdminResponseDto
            {
                Token = token,
                FullName = admin.FullName,
                Email = admin.Email,
                Role = "SystemAdmin"
            };
        }

        private string GenerateJwtToken(SystemAdmin admin)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Name, admin.FullName),
                new Claim(ClaimTypes.Role, "SystemAdmin")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}