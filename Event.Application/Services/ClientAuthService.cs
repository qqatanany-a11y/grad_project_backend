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
    public class ClientAuthService : IClientAuthService
    {
        private readonly IClientRepo _clientRepo;
        private readonly IConfiguration _config;

        public ClientAuthService(IClientRepo clientRepo, IConfiguration config)
        {
            _clientRepo = clientRepo;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existing = await _clientRepo.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("البريد الإلكتروني مسجل مسبقاً");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var client = new Client(
                dto.FirstName,
                dto.LastName,
                dto.Email,
                passwordHash,
                dto.PhoneNumber,
                dto.MiddleName
            );

            await _clientRepo.AddAsync(client);
            var token = GenerateJwtToken(client);

            return new AuthResponseDto
            {
                Token = token,
                FullName = client.FullName,
                Email = client.Email,
                Role = "Client"
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var client = await _clientRepo.GetByEmailAsync(dto.Email);
            if (client == null)
                throw new Exception("البريد الإلكتروني أو كلمة المرور غلط");

            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, client.PasswordHash);
            if (!isValid)
                throw new Exception("البريد الإلكتروني أو كلمة المرور غلط");

            var token = GenerateJwtToken(client);

            return new AuthResponseDto
            {
                Token = token,
                FullName = client.FullName,
                Email = client.Email,
                Role = "Client"
            };
        }

        private string GenerateJwtToken(Client client)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                new Claim(ClaimTypes.Email, client.Email),
                new Claim(ClaimTypes.Name, client.FullName),
                new Claim(ClaimTypes.Role, "Client")
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