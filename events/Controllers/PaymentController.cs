using Event.Application.Dtos;
using Event.Application.IServices;
using events.Helpers;
using events.Services;
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
        private readonly MediaStorageService _mediaStorageService;

        public PaymentController(IPaymentService paymentService, MediaStorageService mediaStorageService)
        {
            _paymentService = paymentService;
            _mediaStorageService = mediaStorageService;
        }

        [HttpPost("pay")]
        public async Task<IActionResult> Pay()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User not authenticated");

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var (dto, form) = await RequestDtoReader.ReadAsync<PayBookingDto>(Request);
                if (form != null)
                {
                    var cliqTransferImageFile = form.Files.GetFile("cliqTransferImageFile");
                    if (cliqTransferImageFile != null)
                    {
                        dto.CliqTransferImageDataUrl = await _mediaStorageService.SaveUploadedImageAsync(
                            cliqTransferImageFile,
                            "payments");
                    }
                }

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
