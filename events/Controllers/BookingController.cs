using Event.Application.Dtos;
using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized("User not authenticated");

        if (!int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized("Invalid user id");

        try
        {
            var result = await _bookingService.CreateBooking(userId, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpGet("my")]
    public async Task<IActionResult> MyBookings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized("User not authenticated");

        var userId = int.Parse(userIdClaim.Value);

        try
        {
            var result = await _bookingService.GetMyBookings(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{bookingId}/cancel")]
    public async Task<IActionResult> Cancel(int bookingId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized("User not authenticated");

        if (!int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized("Invalid user id");

        try
        {
            var result = await _bookingService.Cancel(bookingId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}