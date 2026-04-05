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
    public class UserAuthService : IUserAuthService 
    {
        private readonly IConfiguration _config;
        private readonly IUserRepo _userRepo;

        public UserAuthService(IUserRepo userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("The email is already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
 
            var user=new User(dto.Email, passwordHash, dto.PhoneNumber, dto.FirstName, dto.LastName, dto.MiddleName,2);

            var res=await _userRepo.AddUserAsync(user);
            if (!res)
            {
                throw new Exception("Failed to create user");
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = "User"
            };
        }
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("The email or password is not valid");

            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isValid)
                throw new Exception("The email or password is not valid");

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = "User"
            };
        }

        private string GenerateJwtToken(User client)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                new Claim(ClaimTypes.Email, client.Email),
                new Claim(ClaimTypes.Name, client.FullName),
                new Claim(ClaimTypes.Role, "User")
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