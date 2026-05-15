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
        private readonly IConfiguration _config;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<RegisterOwnerDto> _registerOwnerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly IEmailService _emailService;
        private readonly IOwnerRequestRepo _ownerRequestRepo;

        public AuthService(
            IUserRepo userRepo,
            IRoleRepo roleRepo,
            ICompanyRepo companyRepo,
            IConfiguration config,
            IValidator<RegisterDto> registerValidator,
            IValidator<RegisterOwnerDto> registerOwnerValidator,
            IValidator<LoginDto> loginValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IPasswordGenerator passwordGenerator,
            IEmailService emailService,
            IOwnerRequestRepo ownerRequestRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _companyRepo = companyRepo;
            _config = config;
            _registerValidator = registerValidator;
            _registerOwnerValidator = registerOwnerValidator;
            _loginValidator = loginValidator;
            _changePasswordValidator = changePasswordValidator;
            _passwordGenerator = passwordGenerator;
            _emailService = emailService;
            _ownerRequestRepo = ownerRequestRepo;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
            {
                throw new Exception("Email already exists");
            }

            var role = await _roleRepo.GetRoleByNameAsync("User");
            if (role == null)
            {
                throw new Exception("Role not found");
            }

            var user = new User(
                dto.Email,
                BCrypt.Net.BCrypt.HashPassword(dto.Password),
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                role.Id);

            await _userRepo.AddUserAsync(user);

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user, role.Name),
                FullName = user.FullName,
                Email = user.Email,
                Role = role.Name,
                CompanyId = null,
                IsFirstLogin = false,
                RequiresPasswordChange = false,
                IsOwner = false
            };
        }

        public async Task<AuthResponseDto> RegisterAdminAsync(RegisterDto dto)
        {
            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
            {
                throw new Exception("Email already exists");
            }

            var role = await _roleRepo.GetRoleByNameAsync("Admin");
            if (role == null)
            {
                throw new Exception("Role not found");
            }

            var user = new User(
                dto.Email,
                BCrypt.Net.BCrypt.HashPassword(dto.Password),
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                role.Id);

            await _userRepo.AddUserAsync(user);

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user, role.Name),
                FullName = user.FullName,
                Email = user.Email,
                Role = role.Name,
                CompanyId = null,
                IsFirstLogin = false,
                RequiresPasswordChange = false,
                IsOwner = false
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new Exception("Incorrect email or password");
            }

            var role = await _roleRepo.GetRoleByIdAsync(user.RoleId);
            var roleName = role?.Name ?? "User";
            var isFirstLogin = user.LastLoginAt == null;
            var requiresPasswordChange = RequiresPasswordChange(roleName, isFirstLogin);

            int? companyId = null;
            if (roleName == "Owner")
            {
                var company = await _companyRepo.GetByUserIdAsync(user.Id);
                companyId = company?.Id;
            }

            user.RecordLogin();
            await _userRepo.UpdateUserAsync();

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user, roleName, companyId),
                FullName = user.FullName,
                Email = user.Email,
                Role = roleName,
                CompanyId = companyId,
                IsFirstLogin = requiresPasswordChange,
                RequiresPasswordChange = requiresPasswordChange,
                IsOwner = roleName == "Owner"
            };
        }

        public async Task<AuthResponseDto> RegisterOwnerAsync(RegisterOwnerDto dto)
        {
            var validationResult = await _registerOwnerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
            {
                throw new Exception("Email already exists");
            }

            var request = new OwnerRequest(
                dto.Email,
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                dto.CompanyName,
                dto.BusinessAddress,
                dto.BusinessPhone);

            await _ownerRequestRepo.AddAsync(request);

            await _emailService.SendEmailAsync(
                request.Email,
                "Owner Request Received",
                $@"
<p>Dear {request.FirstName},</p>
<p>Your owner registration request has been received successfully.</p>
<p>We will review your request within <strong>24-48 hours</strong>.</p>
<p>Best regards,<br/>Events Team</p>");

            return new AuthResponseDto
            {
                Token = null,
                FullName = $"{dto.FirstName} {dto.LastName}",
                Email = dto.Email,
                Role = "PendingOwner",
                CompanyId = null,
                IsFirstLogin = false,
                RequiresPasswordChange = false,
                IsOwner = true
            };
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var validationResult = await _changePasswordValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                throw new Exception("Current password is incorrect");
            }

            user.changePassword(BCrypt.Net.BCrypt.HashPassword(dto.NewPassword));
            await _userRepo.UpdateUserAsync();
        }

        private string GenerateJwtToken(User user, string roleName, int? companyId = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, roleName)
            };

            if (companyId.HasValue)
            {
                claims.Add(new Claim("CompanyId", companyId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static bool RequiresPasswordChange(string roleName, bool isFirstLogin)
        {
            return roleName == "Owner" && isFirstLogin;
        }
    }
}
