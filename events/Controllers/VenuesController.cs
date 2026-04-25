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
    // api -> OwnerController -> /api/venues ->  role owner or user
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }



        [HttpGet("venues")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVenues()
        {

            var venues = await _venueService.GetAllAsync();
            return Ok(venues);
        }

  
        [HttpPost("venues")]
        public async Task<IActionResult> AddVenue(AddVenueDto dto)
        {
            return BadRequest("Submit a venue request for admin approval.");
        }


        [HttpPut("venues/{id}")]
        public IActionResult UpdateVenue(int id, UpdateVenueDto dto)
        {
            return BadRequest("Submit a venue edit request for admin approval.");
        }

        [HttpDelete("venues/{id}")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            try
            {
                await _venueService.DeleteAsync(id);
                return Ok("VENUE DELETED SUCSSEFULLY");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpGet("venues/{id}")]
        [AllowAnonymous]

        public async Task<IActionResult> GetVenueById(int id)
        {
            try
            {
                var venue = await _venueService.GetByIdAsync(id);
                if (venue == null)
                    return NotFound("القاعة غير موجودة");
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
