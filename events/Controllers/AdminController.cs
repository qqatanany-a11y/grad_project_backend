using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using System.Text.Json;

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
        private readonly IVenueService _venueService;
        private readonly IEditRequestService _editRequestService;

        public AdminController(
            IAdminService adminService,
            IUserService userService,
            ICompanyRepo companyRepo,
            IVenueService venueService,
            IEditRequestService editRequestService)
        {
            _adminService = adminService;
            _userService = userService;
            _companyRepo = companyRepo;
            _venueService = venueService;
            _editRequestService = editRequestService;
        }

        // ================= OWNER REQUESTS =================

        [HttpGet("owner-requests")]
        public async Task<IActionResult> GetOwnerRequests()
            => Ok(await _adminService.GetOwnerRequestsAsync());

        [HttpPost("owner-requests/{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            await _adminService.ApproveOwnerAsync(id);
            return Ok("Approved successfully");
        }
        [HttpPost("owner-requests/{id}/reject")]
        public async Task<IActionResult> Reject(
            int id,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonElement? payload = null)
        {
            try
            {
                await _adminService.RejectOwnerAsync(id, ExtractReason(payload));
                return Ok("Rejected successfully");
            }
            catch (Exception ex)
            {
                return HandleRejectFailure(ex);
            }
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
            => Ok(await _venueService.GetAllAsync());

        // ================= EDIT REQUESTS =================

        [HttpGet("edit-requests")]
        public async Task<IActionResult> GetEditRequests()
        {
            var result = await _editRequestService.GetAllRequestsAsync();
            return Ok(result);
        }

        [HttpPost("edit-requests/{id}/approve")]
        public async Task<IActionResult> ApproveEditRequest(int id)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null)
                return Unauthorized("Admin not authenticated");

            var adminId = int.Parse(adminIdClaim.Value);

            

            try
            {
                await _editRequestService.ApproveAsync(id, adminId);
                return Ok("Edit request approved");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edit-requests/{id}/reject")]
        public async Task<IActionResult> RejectEditRequest(
            int id,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonElement? payload = null)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null)
                return Unauthorized("Admin not authenticated");

            var adminId = int.Parse(adminIdClaim.Value);


            try
            {
                await _editRequestService.RejectAsync(id, adminId, ExtractReason(payload));
                return Ok("Edit request rejected");
            }
            catch (Exception ex)
            {
                return HandleRejectFailure(ex);
            }
        }
        [HttpPut("edit-requests/{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveEditRequest2(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _adminService.ApproveVenueUpdate(id, adminId);

            return Ok("Approved successfully");
        }
        [HttpPut("edit-requests/{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectEditRequest2(
            int id,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonElement? payload = null)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                await _adminService.RejectVenueUpdate(id, adminId, ExtractReason(payload));
            }
            catch (Exception ex)
            {
                return HandleRejectFailure(ex);
            }

            return Ok("Rejected successfully");
        }

        private IActionResult HandleRejectFailure(Exception ex)
        {
            return ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(ex.Message)
                : BadRequest(ex.Message);
        }

        private static string? ExtractReason(JsonElement? payload)
        {
            if (!payload.HasValue)
            {
                return null;
            }

            var value = payload.Value;
            if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }

            if (value.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in value.EnumerateObject())
            {
                if (!property.Name.Equals("reason", StringComparison.OrdinalIgnoreCase) &&
                    !property.Name.Equals("rejectionReason", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return property.Value.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.Undefined => null,
                    JsonValueKind.String => property.Value.GetString(),
                    _ => property.Value.ToString()
                };
            }

            return null;
        }

    }
}
