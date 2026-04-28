using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entites;
using events.domain.Entities;
using events.domain.Repos;
using FluentValidation;
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
        private readonly ICompanyRepo _companyRepo;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config; private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<RegisterOwnerDto> _registerOwnerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly IEmailService _emailService;

        public AuthService(
            IUserRepo userRepo,
            IRoleRepo roleRepo,
            ICompanyRepo companyRepo,
Microsoft.Extensions.Configuration.IConfiguration config, IValidator<RegisterDto> registerValidator,
            IValidator<RegisterOwnerDto> registerOwnerValidator,
            IValidator<LoginDto> loginValidator,
            IPasswordGenerator passwordGenerator,
            IEmailService emailService
        )
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _companyRepo = companyRepo;
            _config = config;
            _registerValidator = registerValidator;
            _registerOwnerValidator = registerOwnerValidator;
            _loginValidator = loginValidator;
            _passwordGenerator = passwordGenerator;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAdminAsync(RegisterDto dto)
        {
            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("Email already exists");

            var role = await _roleRepo.GetRoleByNameAsync("Admin");
            if (role == null)
                throw new Exception("Role not found");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User(
                dto.Email,
                passwordHash,
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
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

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

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

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Incorrect Email or Password");

            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isValid)
                throw new Exception("Incorrect Email or Password");

            var role = await _roleRepo.GetRoleByIdAsync(user.RoleId);
            var roleName = role?.Name ?? "User";

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
                CompanyId = companyId
            };
        }

        public async Task<AuthResponseDto> RegisterOwnerAsync(RegisterOwnerDto dto)
        {
            var validationResult = await _registerOwnerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("Email already exists");

            var role = await _roleRepo.GetRoleByNameAsync("Owner");
            if (role == null)
                throw new Exception("Role 'Owner' not found");

            string rawPassword = _passwordGenerator.Generate(12);
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

            var user = new User(
                dto.Email,
                passwordHash,
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                role.Id
            );

            await _userRepo.AddUserAsync(user);

            var company = new Company(
                name: dto.CompanyName,
                location: dto.BusinessAddress,
                phoneNumber: dto.BusinessPhone,
                email: dto.Email,
                userId: user.Id
            );

            await _companyRepo.AddCompanyAsync(company);

            string emailSubject = "بيانات الدخول لحسابك الجديد";
            string emailBody =
                $"مرحباً {user.FullName},\n\n" +
                $"تم إنشاء حساب المالك بنجاح.\n\n" +
                $"كلمة المرور: {rawPassword}\n\n" +
                $"يرجى تغييرها بعد تسجيل الدخول.";

            await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

            var token = GenerateJwtToken(user, role.Name, company.Id);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = role.Name,
                CompanyId = company.Id
            };
        }

        private string GenerateJwtToken(User user, string roleName, int? companyId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, roleName)
            };

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
    }
}