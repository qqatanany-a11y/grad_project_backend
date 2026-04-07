using Event.Application.Dtos;
using events.domain.Entites;
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
        private readonly ICompanyRepo _companyRepo;    // ← جديد
        private readonly IConfiguration _config;

        public AuthService(IUserRepo userRepo, IRoleRepo roleRepo,
                           ICompanyRepo companyRepo,               // ← جديد
                           IConfiguration config)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _companyRepo = companyRepo;                            // ← جديد
            _config = config;
        }

        // RegisterAsync — ما تغير عليه شي ✅

        public async Task<AuthResponseDto> RegisterOwnerAsync(RegisterOwnerDto dto)
        {
            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("البريد الإلكتروني مسجل مسبقاً");

            // ← صلحنا: بنجيب الـ Role من الـ DB مش Hardcoded
            var role = await _roleRepo.GetRoleByNameAsync("Owner");
            if (role == null)
                throw new Exception("الـ Role غير موجود");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User(
                dto.Email, passwordHash, dto.PhoneNumber,
                dto.FirstName, dto.LastName, dto.MiddleName, role.Id
            );
            await _userRepo.AddUserAsync(user);

            // ← جديد: بنحفظ الشركة وبنربطها بالـ User
            var company = new Company(
                dto.CompanyName,
                dto.BusinessAddress,
                dto.BusinessPhone,
                dto.Email,
                user.Id                   // ← UserId تبع الـ Owner
            );
            await _companyRepo.AddAsync(company);

            // ← جديد: بنحط CompanyId بالـ JWT
            var token = GenerateJwtToken(user, role.Name, company.Id);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = role.Name,
                CompanyId = company.Id    // ← جديد
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Inccorect Email or Password");

            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isValid)
                throw new Exception("Inccorect Email or Password");

            var role = await _roleRepo.GetRoleByIdAsync(user.RoleId);
            var roleName = role?.Name ?? "User";

            // ← إذا Owner، بنجيب الـ CompanyId تبعه
            int? companyId = null;
            if (roleName == "Owner")
            {
                var company = await _companyRepo.GetByUserIdAsync(user.Id);
                companyId = company?.Id;
            }

            var token = GenerateJwtToken(user, roleName, companyId);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = roleName,
                CompanyId = companyId     // ← جديد
            };
        }

        // ← معدل: أضفنا companyId للـ JWT
        private string GenerateJwtToken(User user, string roleName, int? companyId = null)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, roleName)
        };

            // ← بنضيف CompanyId للـ Token بس إذا كان Owner
            if (companyId.HasValue)
                claims.Add(new Claim("CompanyId", companyId.Value.ToString()));

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

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
{
    var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
    if (existing != null)
        throw new Exception("Email already exists");

    var role = await _roleRepo.GetRoleByNameAsync("User");
    if (role == null)
        throw new Exception("Role not found");

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

    var token = GenerateJwtToken(user, role.Name);

    return new AuthResponseDto
    {
        Token = token,
        FullName = user.FullName,
        Email = user.Email,
        Role = role.Name,
        CompanyId = null
    };
}
    }
}