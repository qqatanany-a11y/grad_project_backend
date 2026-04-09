using Event.Application.Dtos;
using Event.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace events.Controllers
{
    [ApiController]
    [Route("api/auth")]  // ← غيرنا الـ Route لـ auth لأنو Login للكل
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;  // ← وحدنا الـ Service

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/register → للـ User العادي
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/auth/login → للكل (User, Owner, Admin)
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}