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

    }
}