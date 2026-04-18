using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/owner/bookings")]
[Authorize(Roles = "Owner")]
public class OwnerBookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public OwnerBookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (ownerIdClaim == null)
            return Unauthorized("Owner not authenticated");

        var ownerId = int.Parse(ownerIdClaim.Value);

        try
        {
            var result = await _bookingService.GetOwnerBookings(ownerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (ownerIdClaim == null)
            return Unauthorized("Owner not authenticated");

        var ownerId = int.Parse(ownerIdClaim.Value);

        try
        {
            await _bookingService.Approve(id, ownerId);
            return Ok("Approved");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (ownerIdClaim == null)
            return Unauthorized("Owner not authenticated");

        var ownerId = int.Parse(ownerIdClaim.Value);

        try
        {
            await _bookingService.Reject(id, ownerId);
            return Ok("Rejected");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}