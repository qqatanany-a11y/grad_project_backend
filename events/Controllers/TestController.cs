using Microsoft.AspNetCore.Mvc;

namespace events.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult test()
        {
            return Ok("Test successful");
        }
    }
}
