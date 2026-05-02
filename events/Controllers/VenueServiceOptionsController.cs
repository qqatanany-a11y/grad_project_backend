using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace events.Controllers
{
    [ApiController]
    [Route("api/venues/{venueId}/service-options")]
    public class VenueServiceOptionsController : ControllerBase
    {
        private readonly IVenueServiceOptionService _venueServiceOptionService;

        public VenueServiceOptionsController(IVenueServiceOptionService venueServiceOptionService)
        {
            _venueServiceOptionService = venueServiceOptionService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByVenueId(int venueId)
        {
            try
            {
                var result = await _venueServiceOptionService.GetByVenueIdAsync(venueId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
