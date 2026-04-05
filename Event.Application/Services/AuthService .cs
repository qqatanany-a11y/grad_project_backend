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
    public class AuthService : IAuthService
    {
        private readonly IUserRepo _userRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepo userRepo, IRoleRepo roleRepo,
                           IConfiguration config)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("البريد الإلكتروني مسجل مسبقاً");

            var role = await _roleRepo.GetRoleByNameAsync("User");
            if (role == null)
                throw new Exception("الـ Role غير موجود");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User(
                dto.Email,
                passwordHash,
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                dto.MiddleName,
                role.Id
            );

            await _userRepo.AddUserAsync(user);
            var token = GenerateJwtToken(user, role.Name); // ← معدل

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = role.Name
            };
        }

        public async Task<AuthResponseDto> RegisterOwnerAsync(RegisterOwnerDto dto)
        {
            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("البريد الإلكتروني مسجل مسبقاً");

   

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User(
                dto.Email,
                passwordHash,
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                dto.MiddleName,
                3
               );

            await _userRepo.AddUserAsync(user);

            //var ownerProfile = new OwnerProfile(
            //    user.Id,
            //    dto.CompanyName,
            //    dto.BusinessPhone,
            //    dto.BusinessAddress
            //);

            //await _ownerProfileRepo.AddAsync(ownerProfile);

            var token = GenerateJwtToken(user, "Owner"); // ← معدل

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = "Owner"
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("البريد الإلكتروني أو كلمة المرور غلط");

            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isValid)
                throw new Exception("البريد الإلكتروني أو كلمة المرور غلط");

            var role = await _roleRepo.GetRoleByIdAsync(user.RoleId);
            var roleName = role?.Name ?? "User";

            var token = GenerateJwtToken(user, roleName); // ← معدل

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = roleName
            };
        }

        private string GenerateJwtToken(User user, string roleName)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, roleName) // ← معدل
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