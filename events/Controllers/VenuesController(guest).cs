using Event.Application.Dtos;
using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace events.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
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

        
       
    }
}