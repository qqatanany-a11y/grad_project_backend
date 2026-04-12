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
    [Authorize(Roles = "Owner")]  // ← كل الـ Endpoints محمية للـ Owner بس
    public class OwnerController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IVenueService _venueService;
        private readonly ICompanyRepo _companyRepo;

        public OwnerController(IAuthService authService, IVenueService venueService, ICompanyRepo companyRepo)
        {
            _authService = authService;
            _venueService = venueService;
            _companyRepo = companyRepo;
        }

        // POST api/owner/register → مش محتاج Authorize لأنو تسجيل جديد
        [HttpPost("register")]
        [AllowAnonymous]  // ← استثناء عشان التسجيل مالو Auth
        public async Task<IActionResult> RegisterOwner(RegisterOwnerDto dto)
        {
            try
            {
                var result = await _authService.RegisterOwnerAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/owner/me → بيرجع معلومات الـ Owner من الـ Token
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
        private int GetCompanyId() =>
    int.Parse(User.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value ?? "0");

        [HttpGet("venues")]
        public async Task<IActionResult> GetMyVenues()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();
            var venues = await _venueService.GetByCompanyIdAsync(companyId);
            return Ok(venues);
        }

        [HttpGet("venues/{id}")]
        public async Task<IActionResult> GetVenueDetails(int id)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();
            var venues = await _venueService.GetByCompanyIdAsync(companyId);
            var venue = venues.FirstOrDefault(v => v.Id == id);
            if (venue == null) return NotFound("Venue not found");
            return Ok(venue);
        }

        [HttpPost("venues")]
        public async Task<IActionResult> RegisterVenue(AddVenueDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();
            try
            {
                var result = await _venueService.AddAsync(dto, companyId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("venues/availability")]
        public async Task<IActionResult> GetVenuesAvailability()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();
            var venues = await _companyRepo.GetVenuesByCompanyIdAsync(companyId);
            var result = venues.SelectMany(v => v.Availabilities.Select(a => new VenueAvailabilityDto
            {
                Id = a.Id,
                VenueId = v.Id,
                VenueName = v.Name,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsAvailable = a.IsAvailable
            })).ToList();
            return Ok(result);
        }
    }
}