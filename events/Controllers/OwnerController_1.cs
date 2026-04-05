using Event.Application.Dtos;
using Event.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace events.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerController : ControllerBase
    {
        private readonly IAuthService _authService;

        public OwnerController(IAuthService authService)
        {
            _authService = authService;
        }

 
        [HttpPost("register-owner")]
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

        [HttpGet("test")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public object Test()
        {
            var companyId = User.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            return new
            {
                CompanyId = companyId,
                Role = role,
                Name = name
            };

        }
    }
}