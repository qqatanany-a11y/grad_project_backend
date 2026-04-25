using Event.Application.Dtos;
using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace events.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<List<VenueDto>>> GetAllForGuest()
        {
            var venues = await _venueService.GetVenuesForGuestAsync();
            return Ok(venues);
        }

        [HttpGet("venues")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVenues()
        {
            var venues = await _venueService.GetAllAsync();
            return Ok(venues);
        }

        [HttpPost("venues")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> AddVenue(AddVenueDto dto)
        {
            var companyIdClaim = User.FindFirst("CompanyId");

            if (companyIdClaim == null)
                return Unauthorized("Owner company not found");

            var companyId = int.Parse(companyIdClaim.Value);

            try
            {
                var result = await _venueService.AddAsync(companyId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateVenue(int id, UpdateVenueDto dto)
        {
            try
            {
                var result = await _venueService.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            try
            {
                await _venueService.DeleteAsync(id);
                return Ok("VENUE DELETED SUCCESSFULLY");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVenueById(int id)
        {
            try
            {
                var venue = await _venueService.GetByIdAsync(id);

                if (venue == null)
                    return NotFound("Venue not found");

                return Ok(venue);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("VienuesByOwnerId/{ownerId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetVenuesByOwnerId(int ownerId)
        {
            try
            {
                var venues = await _venueService.GetByOwnerIdAsync(ownerId);
                return Ok(venues);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("VienuesByCompanyId/{companyId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetVenuesByCompanyId(int companyId)
        {
            try
            {
                var venues = await _venueService.GetByCompanyIdAsync(companyId);
                return Ok(venues);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}