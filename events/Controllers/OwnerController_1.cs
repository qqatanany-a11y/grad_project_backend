using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Repos;
using events.Helpers;
using events.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace events.Controllers
{
    [ApiController]
    [Route("api/owner")]
    [Authorize(Roles = "Owner")]
    public class OwnerController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IVenueService _venueService;
        private readonly ICompanyRepo _companyRepo;
        private readonly IEditRequestService _editRequestService;
        private readonly IAdminService _adminService;
        private readonly MediaStorageService _mediaStorageService;

        public OwnerController(
            IAuthService authService,
            IVenueService venueService,
            ICompanyRepo companyRepo,
            IAdminService adminService,
            IEditRequestService editRequestService,
            MediaStorageService mediaStorageService)
        {
            _authService = authService;
            _adminService = adminService;
            _venueService = venueService;
            _companyRepo = companyRepo;
            _editRequestService = editRequestService;
            _mediaStorageService = mediaStorageService;
        }

        [HttpGet("me")]
        public IActionResult GetMyInfo()
        {
            var companyId = User.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            return Ok(new
            {
                CompanyId = companyId,
                Role = role,
                Name = name
            });
        }

        [HttpPost("request")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOwnerRequest(RegisterOwnerDto dto)
        {
            try
            {
                await _adminService.OwnerRequestAsync(dto);

                return Ok("Request sent, waiting for approval");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edit-requests/profile")]
        public async Task<IActionResult> CreateProfileEditRequest(ProfileEditRequestDto dto)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
                return Unauthorized("Owner not authenticated");

            var ownerId = int.Parse(ownerIdClaim.Value);

            try
            {
                await _editRequestService.CreateProfileEditRequestAsync(ownerId, dto);
                return Ok("Profile edit request submitted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edit-requests/venue/{venueId}")]
        public async Task<IActionResult> CreateVenueEditRequest(int venueId)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
                return Unauthorized("Owner not authenticated");

            var ownerId = int.Parse(ownerIdClaim.Value);

            try
            {
                var (dto, form) = await RequestDtoReader.ReadAsync<VenueEditRequestDto>(Request);
                await ApplyUploadedVenueImagesAsync(dto, form);
                await _editRequestService.CreateVenueEditRequestAsync(ownerId, venueId, dto);
                return Ok("Venue edit request submitted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edit-requests/venue-create")]
        public async Task<IActionResult> CreateVenueCreateRequest()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
                return Unauthorized("Owner not authenticated");

            var ownerId = int.Parse(ownerIdClaim.Value);

            try
            {
                var (dto, form) = await RequestDtoReader.ReadAsync<CreateVenueRequestDto>(Request);
                await ApplyUploadedVenueImagesAsync(dto, form);
                await _editRequestService.CreateVenueCreateRequestAsync(ownerId, dto);
                return Ok("Venue creation request submitted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("edit-requests/my")]
        public async Task<IActionResult> MyEditRequests()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
                return Unauthorized("Owner not authenticated");

            var ownerId = int.Parse(ownerIdClaim.Value);

            try
            {
                var result = await _editRequestService.GetMyRequestsAsync(ownerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task ApplyUploadedVenueImagesAsync(VenueEditRequestDto dto, IFormCollection form)
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

        private async Task ApplyUploadedVenueImagesAsync(CreateVenueRequestDto dto, IFormCollection form)
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
