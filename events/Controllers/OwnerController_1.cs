using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace events.Controllers
{
    [ApiController]
    [Route("api/owner")]
    [Authorize(Roles = "Owner")]
    public class OwnerController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IVenueService _venueService;
        private readonly ICompanyRepo _companyRepo;
        private readonly IOwnerRequestRepo _ownerRequestRepo;
        private readonly IEditRequestService _editRequestService;
        private readonly IAdminService _adminService;

        public OwnerController(
            IAuthService authService,
            IVenueService venueService,
            ICompanyRepo companyRepo,
            IOwnerRequestRepo ownerRequestRepo,
            IAdminService adminService,
            IEditRequestService editRequestService)
        {
            _authService = authService;
            _adminService = adminService;
            _venueService = venueService;
            _companyRepo = companyRepo;
            _ownerRequestRepo = ownerRequestRepo;
            _editRequestService = editRequestService;
        }

        [HttpGet("me")]
        public IActionResult GetMyInfo()
        {
            var companyId = User.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            return Ok(new
            {
                CompanyId = companyId,
                Role = role,
                Name = name
            });
        }

        [HttpPost("request")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOwnerRequest(RegisterOwnerDto dto)
        {
            try
            {
                await _adminService.OwnerRequestAsync(dto);
 
                return Ok("Request sent, waiting for approval");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edit-requests/profile")]
        public async Task<IActionResult> CreateProfileEditRequest(ProfileEditRequestDto dto)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
                return Unauthorized("Owner not authenticated");

            var ownerId = int.Parse(ownerIdClaim.Value);

            try
            {
                await _editRequestService.CreateProfileEditRequestAsync(ownerId, dto);
                return Ok("Profile edit request submitted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edit-requests/venue/{venueId}")]
        public async Task<IActionResult> CreateVenueEditRequest(int venueId, VenueEditRequestDto dto)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
                return Unauthorized("Owner not authenticated");

            var ownerId = int.Parse(ownerIdClaim.Value);

            try
            {
                await _editRequestService.CreateVenueEditRequestAsync(ownerId, venueId, dto);
                return Ok("Venue edit request submitted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("edit-requests/my")]
        public async Task<IActionResult> MyEditRequests()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
                return Unauthorized("Owner not authenticated");

            var ownerId = int.Parse(ownerIdClaim.Value);

            try
            {
                var result = await _editRequestService.GetMyRequestsAsync(ownerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}