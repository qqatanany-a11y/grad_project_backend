using Event.Application.Dtos;
using Event.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace events.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminAuthService _adminAuthService;

        public AdminController(IAdminAuthService adminAuthService)
        {
            _adminAuthService = adminAuthService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AdminRegisterDto dto)
        {
            try
            {
                var result = await _adminAuthService.RegisterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto dto)
        {
            try
            {
                var result = await _adminAuthService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}