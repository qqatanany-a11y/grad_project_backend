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
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;
        private readonly MediaStorageService _mediaStorageService;

        public VenuesController(IVenueService venueService, MediaStorageService mediaStorageService)
        {
            _venueService = venueService;
            _mediaStorageService = mediaStorageService;
        }

        [HttpGet]
        [HttpGet("all")]
        [HttpGet("venues")]
        [AllowAnonymous]
        public async Task<ActionResult<List<VenueDto>>> GetAllForGuest()
        {
            var venues = await _venueService.GetVenuesForGuestAsync();
            return Ok(venues);
        }

        [HttpPost]
        [HttpPost("venues")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> AddVenue()
        {
            var companyIdClaim = User.FindFirst("CompanyId");
            if (companyIdClaim == null)
            {
                return Unauthorized("Owner company not found");
            }

            try
            {
                var (dto, form) = await RequestDtoReader.ReadAsync<AddVenueDto>(Request);
                await ApplyUploadedVenueImagesAsync(dto, form);
                var result = await _venueService.AddAsync(int.Parse(companyIdClaim.Value), dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [HttpPut("venues/{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateVenue(int id)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
            {
                return Unauthorized("Owner not authenticated");
            }

            try
            {
                var (dto, form) = await RequestDtoReader.ReadAsync<UpdateVenueDto>(Request);
                await ApplyUploadedVenueImagesAsync(dto, form);
                var result = await _venueService.UpdateAsync(int.Parse(ownerIdClaim.Value), id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [HttpDelete("venues/{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            try
            {
                await _venueService.DeleteAsync(id);
                return Ok("Venue deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [HttpGet("venues/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVenueById(int id)
        {
            try
            {
                var venue = await _venueService.GetByIdAsync(id);
                return Ok(venue);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("owner/{ownerId}")]
        [HttpGet("VienuesByOwnerId/{ownerId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetVenuesByOwnerId(int ownerId)
        {
            try
            {
                var venues = await _venueService.GetByOwnerIdAsync(ownerId);
                return Ok(venues);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("company/{companyId}")]
        [HttpGet("VienuesByCompanyId/{companyId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetVenuesByCompanyId(int companyId)
        {
            try
            {
                var venues = await _venueService.GetByCompanyIdAsync(companyId);
                return Ok(venues);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] VenueQueryParams query)
        {
            try
            {
                var result = await _venueService.SearchAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task ApplyUploadedVenueImagesAsync(AddVenueDto dto, IFormCollection form)
        {
            if (form == null)
            {
                return;
            }

            var uploadedPathsByToken = await _mediaStorageService.SaveUploadedImagesByTokenAsync(
                form["photoTokens"],
                form.Files.GetFiles("photoFiles"),
                "venues");

            dto.ImageUrls = VenueUploadTokenResolver.ReplaceTokens(dto.ImageUrls, uploadedPathsByToken);
            dto.CoverPhotoDataUrl = VenueUploadTokenResolver.ReplaceToken(dto.CoverPhotoDataUrl, uploadedPathsByToken);
            dto.GalleryPhotoDataUrls = VenueUploadTokenResolver.ReplaceTokens(dto.GalleryPhotoDataUrls, uploadedPathsByToken);
            dto.PhotoDataUrls = VenueUploadTokenResolver.ReplaceTokens(dto.PhotoDataUrls, uploadedPathsByToken);
        }

        private async Task ApplyUploadedVenueImagesAsync(UpdateVenueDto dto, IFormCollection form)
        {
            if (form == null)
            {
                return;
            }

            var uploadedPathsByToken = await _mediaStorageService.SaveUploadedImagesByTokenAsync(
                form["photoTokens"],
                form.Files.GetFiles("photoFiles"),
                "venues");

            dto.ImageUrls = VenueUploadTokenResolver.ReplaceTokens(dto.ImageUrls, uploadedPathsByToken);
            dto.CoverPhotoDataUrl = VenueUploadTokenResolver.ReplaceToken(dto.CoverPhotoDataUrl, uploadedPathsByToken);
            dto.GalleryPhotoDataUrls = VenueUploadTokenResolver.ReplaceTokens(dto.GalleryPhotoDataUrls, uploadedPathsByToken);
            dto.PhotoDataUrls = VenueUploadTokenResolver.ReplaceTokens(dto.PhotoDataUrls, uploadedPathsByToken);
        }
    }
}
