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
        private readonly IConfiguration _config;
        private readonly IUserRepo _userRepo;

        public AdminAuthService(IConfiguration config,IUserRepo repo)
        {
            _config = config;
            _userRepo = repo;
        }
        // admin registration -> edit system admin to user

        public async Task<AdminResponseDto> RegisterAsync(AdminRegisterDto dto)
        {
            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("البريد الإلكتروني مسجل مسبقاً");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var admin = new User(
                dto.Email,
                passwordHash,
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                dto.MiddleName,
                1
            );

            await _userRepo.AddUserAsync(admin);
            var token = GenerateJwtToken(admin);

            return new AdminResponseDto
            {
                Token = token,
                FullName = admin.FullName,
                Email = admin.Email,
                Role = "Admin"
            };
        }

        public async Task<AdminResponseDto> LoginAsync(AdminLoginDto dto)
        {
            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("البريد الإلكتروني أو كلمة المرور غلط");

            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isValid)
                throw new Exception("البريد الإلكتروني أو كلمة المرور غلط");

            var token = GenerateJwtToken(user);

            return new AdminResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = "SystemAdmin"
            };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, "Admin")
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