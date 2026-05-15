using Event.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using System.Text.Json;

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
    public async Task<IActionResult> Reject(
        int id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonElement? payload = null)
    {
        var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (ownerIdClaim == null)
            return Unauthorized("Owner not authenticated");

        var ownerId = int.Parse(ownerIdClaim.Value);

        try
        {
            await _bookingService.Reject(id, ownerId, ExtractReason(payload));
            return Ok("Rejected");
        }
        catch (Exception ex)
        {
            return ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(ex.Message)
                : BadRequest(ex.Message);
        }
    }

    private static string? ExtractReason(JsonElement? payload)
    {
        if (!payload.HasValue)
        {
            return null;
        }

        var value = payload.Value;
        if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var property in value.EnumerateObject())
        {
            if (!property.Name.Equals("reason", StringComparison.OrdinalIgnoreCase) &&
                !property.Name.Equals("rejectionReason", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return property.Value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                JsonValueKind.String => property.Value.GetString(),
                _ => property.Value.ToString()
            };
        }

        return null;
    }
}
