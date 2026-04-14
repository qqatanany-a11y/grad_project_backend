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
    [Authorize(Roles = "Owner")] 
    public class OwnerController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IVenueService _venueService;
        private readonly ICompanyRepo _companyRepo;
        private readonly IOwnerRequestRepo _ownerRequestRepo;

        public OwnerController(IAuthService authService, IVenueService venueService, ICompanyRepo companyRepo, IOwnerRequestRepo ownerRequestRepo)
        {
            _authService = authService;
            _venueService = venueService;
            _companyRepo = companyRepo;
            _ownerRequestRepo = ownerRequestRepo;
        }
        

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
        [HttpPost("request")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOwnerRequest(RegisterOwnerDto dto)
        {
            try
            {
                var request = new OwnerRequest(
                    dto.Email,
                    dto.PhoneNumber,
                    dto.FirstName,
                    dto.LastName,
                    dto.CompanyName,
                    dto.BusinessAddress,
                    dto.BusinessPhone,
                    dto.VenueName
                );

                await _ownerRequestRepo.AddAsync(request);

                return Ok("Request sent, waiting for approval");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}