using Event.Application.Dtos;
using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace events.Controllers
{
    [ApiController]
    [Route("api/venue-availabilities")]
    public class VenueAvailabilityController : ControllerBase
    {
        private readonly IVenueAvailabilityService _venueAvailabilityService;

        public VenueAvailabilityController(IVenueAvailabilityService venueAvailabilityService)
        {
            _venueAvailabilityService = venueAvailabilityService;
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Add(CreateVenueAvailabilityDto dto)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (ownerIdClaim == null)
                return Unauthorized("Owner not authenticated");

            var ownerId = int.Parse(ownerIdClaim.Value);

            try
            {
                var result = await _venueAvailabilityService.AddAsync(ownerId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("owner/{venueId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetByVenueId(int venueId)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (ownerIdClaim == null)
                return Unauthorized("Owner not authenticated");

            var ownerId = int.Parse(ownerIdClaim.Value);

            try
            {
                var result = await _venueAvailabilityService.GetByVenueIdAsync(ownerId, venueId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{venueId}/available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(int venueId, [FromQuery] DateOnly date)
        {
            try
            {
                var result = await _venueAvailabilityService.GetAvailableSlotsAsync(venueId, date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}