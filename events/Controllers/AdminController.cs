using Event.Application.Dtos;
using Event.Application.IServices;
using Event.Application.Services;
using Event.Infrastructure.Repos;
using events.domain.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace events.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICompanyRepo _companyRepo;
        private readonly IAuthService _authService;


        public AdminController(IUserService userService, ICompanyRepo companyRepo, IAuthService authService)
        {
            _userService = userService;
            _companyRepo = companyRepo;
            _authService = authService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAdminAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       

        [Authorize(Roles = "Admin")]

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId);
                if (result)
                    return Ok("User deleted successfully");
                else
                    return NotFound("User not found");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user != null)
                    return Ok(user);
                else
                    return NotFound("User not found");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("id/{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user != null)
                    return Ok(user);
                else
                    return NotFound("User not found");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, UserDto userDto)
        {
            try
            {
                await _userService.UpdateUserAsync(userId, userDto);
                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Admin")]

        [HttpPut("Activate/{userId}")]
        public async Task<IActionResult> ActivateUser(int userId)
        {
            try
            {
                await _userService.ActivateUserAsync(userId);
                return Ok("User activated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Admin")]

        [HttpPut("Deactivate/{userId}")]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            try
            {
                await _userService.DeactivateUserAsync(userId);
                return Ok("User deactivated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("companies/{id}")]
        public async Task<IActionResult> GetCompanyDetails(int id)
        {
            try
            {
                var company = await _companyRepo.GetByIdAsync(id);
                if (company == null) return NotFound("Company not found");

                var result = new CompanyDetailsDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    Location = company.Location,
                    PhoneNumber = company.PhoneNumber,
                    Email = company.Email,
                    Venues = company.Venues?.Select(v => new VenueDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        City = v.City,
                        Capacity = v.Capacity,
                        MinimalPrice = v.MinimalPrice,
                        IsActive = v.IsActive
                    }).ToList()
                };
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("companies")]
        public async Task<IActionResult> GetAllCompanies()
        {
            try
            {
                var companies = await _companyRepo.GetAllAsync();
                var result = companies.Select(c => new CompanyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Location = c.Location,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    VenuesCount = c.Venues?.Count ?? 0
                }).ToList();
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("companies/{id}/venues")]
        public async Task<IActionResult> GetCompanyVenues(int id)
        {
            try
            {
                var venues = await _companyRepo.GetVenuesByCompanyIdAsync(id);
                var result = venues.Select(v => new VenueDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    City = v.City,
                    Address = v.Address,
                    Capacity = v.Capacity,
                    MinimalPrice = v.MinimalPrice,
                    IsActive = v.IsActive
                }).ToList();
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

    }
}
