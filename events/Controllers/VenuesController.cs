using Event.Application.Dtos;
using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace events.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Owner")]
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
            var venues = await _venueService.GetVenuesForGuestAsync();
            return Ok(venues);
        }

        [HttpPost]
        [HttpPost("venues")]
        public async Task<IActionResult> AddVenue(AddVenueDto dto)
        {
            var companyIdClaim = User.FindFirst("companyId");

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
        [HttpPut("venues/{id}")]
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
        [HttpDelete("venues/{id}")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            try
            {
                await _venueService.DeleteAsync(id);
                return Ok("Venue deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [HttpGet("venues/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVenueById(int id)
        {
            try
            {
                var venue = await _venueService.GetByIdAsync(id);
                return Ok(venue);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("VienuesByOwnerId/{OwnerId}")]
        public async Task<IActionResult> GetVenuesByOwnerId(int OwnerId)
        {
            try
            {
                var venues = await _venueService.GetByOwnerIdAsync(OwnerId);
                return Ok(venues);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("VienuesByCompanyId/{CompanyId}")]
        public async Task<IActionResult> GetVenuesByCompanyId(int CompanyId)
        {
            try
            {
                var venues = await _venueService.GetByCompanyIdAsync(CompanyId);
                return Ok(venues);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
