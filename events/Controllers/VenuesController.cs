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

        private int GetCompanyId()
        {
            return int.Parse(User.Claims
                .FirstOrDefault(c => c.Type == "CompanyId")?.Value ?? "0");
        }

        [HttpGet("venues")]
        public async Task<IActionResult> GetVenues()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var venues = await _venueService.GetByCompanyIdAsync(companyId);
            return Ok(venues);
        }

        [HttpPost("venues")]
        public async Task<IActionResult> AddVenue(AddVenueDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            try
            {
                var result = await _venueService.AddAsync(dto, companyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("venues/{id}")]
        public async Task<IActionResult> UpdateVenue(int id, UpdateVenueDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) 
                return Unauthorized();
            try
            {
                var result = await _venueService.UpdateAsync(id, dto, companyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("venues/{id}")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            try
            {
                await _venueService.DeleteAsync(id, companyId);
                return Ok("تم حذف القاعة بنجاح");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}