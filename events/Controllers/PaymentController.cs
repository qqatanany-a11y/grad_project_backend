using Event.Application.Dtos;
using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace events.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("pay")]
        public async Task<IActionResult> Pay(PayBookingDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User not authenticated");

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var result = await _paymentService.PayAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}