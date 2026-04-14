using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly ICompanyRepo _companyRepo;
        private readonly IVenueRepo _venueRepo;

        public AdminController(
            IAdminService adminService,
            IUserService userService,
            ICompanyRepo companyRepo,
            IVenueRepo venueRepo)
        {
            _adminService = adminService;
            _userService = userService;
            _companyRepo = companyRepo;
            _venueRepo = venueRepo;
        }

        // ================= OWNER REQUESTS =================

        [HttpGet("owner-requests")]
        public async Task<IActionResult> GetOwnerRequests()
            => Ok(await _adminService.GetOwnerRequestsAsync());

        [HttpPost("owner-requests/{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            await _adminService.ApproveOwnerAsync(id);
            return Ok("Approved");
        }

        [HttpPost("owner-requests/{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            await _adminService.RejectOwnerAsync(id);
            return Ok("Rejected");
        }

        // ================= USERS =================

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
            => Ok(await _userService.GetAllUsersAsync());

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
            => Ok(await _userService.GetUserByIdAsync(id));

        [HttpGet("users/email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
            => Ok(await _userService.GetUserByEmailAsync(email));

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto dto)
        {
            await _userService.UpdateUserAsync(id, dto);
            return Ok("Updated");
        }

        [HttpPut("users/{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _userService.ActivateUserAsync(id);
            return Ok("Activated");
        }

        [HttpPut("users/{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _userService.DeactivateUserAsync(id);
            return Ok("Deactivated");
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteUserAsync(id);
            return Ok("Deleted");
        }

        // ================= DASHBOARD =================

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
            => Ok(await _companyRepo.GetAllAsync());

        [HttpGet("venues")]
        public async Task<IActionResult> GetVenues()
            => Ok(await _venueRepo.GetAllAsync());
    }
}