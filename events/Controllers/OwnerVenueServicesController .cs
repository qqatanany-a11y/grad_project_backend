using Event.Application.Dtos;
using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace events.Controllers
{
    [ApiController]
    [Route("api/owner/venue-services")]
    [Authorize(Roles = "Owner")]
    public class OwnerVenueServicesController : ControllerBase
    {
        private readonly IVenueServiceOptionService _venueServiceOptionService;

        public OwnerVenueServicesController(IVenueServiceOptionService venueServiceOptionService)
        {
            _venueServiceOptionService = venueServiceOptionService;
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddVenueServiceOptionDto dto)
        {
            try
            {
                var result = await _venueServiceOptionService.AddAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{venueId}")]
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