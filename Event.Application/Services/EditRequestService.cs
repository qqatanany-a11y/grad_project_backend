using System.Text.Json;
using Event.Application.Dtos;
using Event.Application.Helpers;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class EditRequestService : IEditRequestService
    {
        private static readonly JsonSerializerOptions RequestJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IEditRequestRepo _editRequestRepo;
        private readonly IUserRepo _userRepo;
        private readonly IVenueRepo _venueRepo;
        private readonly IEmailService _emailService;
        private readonly ICompanyRepo _companyRepo;

        public EditRequestService(
            IEditRequestRepo editRequestRepo,
            IUserRepo userRepo,
            IEmailService emailService,
            IVenueRepo venueRepo,
            ICompanyRepo companyRepo)
        {
            _editRequestRepo = editRequestRepo;
            _userRepo = userRepo;
            _emailService = emailService;
            _venueRepo = venueRepo;
            _companyRepo = companyRepo;
        }

        public async Task CreateProfileEditRequestAsync(int ownerId, ProfileEditRequestDto dto)
        {
            var owner = await _userRepo.GetUserByIdAsync(ownerId);
            if (owner == null)
            {
                throw new Exception("Owner not found");
            }

            var request = new EditRequest(
                ownerId,
                EditRequestTypeEnum.Profile,
                null,
                JsonSerializer.Serialize(dto, RequestJsonOptions));

            await _editRequestRepo.AddAsync(request);
            await _editRequestRepo.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                owner.Email,
                "Profile Edit Request Submitted",
                $@"
<p>Dear {owner.FirstName},</p>
<p>Your profile edit request has been submitted successfully.</p>
<p>Best regards,<br/>Events Team</p>");
        }

        public async Task CreateVenueEditRequestAsync(int ownerId, int venueId, VenueEditRequestDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);
            if (venue == null)
            {
                throw new Exception("Venue not found");
            }

            if (venue.Company.UserId != ownerId)
            {
                throw new Exception("Not allowed");
            }

            VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

            var requestData = new VenueEditRequestDataDto
            {
                VenueId = venue.Id,
                CompanyName = venue.Company?.Name ?? string.Empty,
                Current = new VenueEditRequestDto
                {
                    Name = venue.Name,
                    Description = venue.Description,
                    City = venue.City,
                    Address = venue.Address,
                    Capacity = venue.Capacity,
                    IsActive = venue.IsActive,
                    Type = venue.Type,
                    Category = venue.Category,
                    PricingType = venue.PricingType,
                    PricePerHour = venue.PricePerHour,
                    DepositPercentage = venue.DepositPercentage,
                    FacebookUrl = venue.FacebookUrl,
                    InstagramUrl = venue.InstagramUrl,
                    WebsiteUrl = venue.WebsiteUrl,
                    TimeSlots = VenueSlotSupport.MapEditableSlots(venue.TimeSlots)
                },
                Requested = dto
            };

            var request = new EditRequest(
                ownerId,
                EditRequestTypeEnum.VenueUpdate,
                venueId,
                JsonSerializer.Serialize(requestData, RequestJsonOptions));

            await _editRequestRepo.AddAsync(request);
            await _editRequestRepo.SaveChangesAsync();
        }

        public async Task CreateVenueCreateRequestAsync(int ownerId, CreateVenueRequestDto dto)
        {
            var owner = await _userRepo.GetUserByIdAsync(ownerId);
            if (owner == null)
            {
                throw new Exception("Owner not found");
            }

            var company = await _companyRepo.GetByUserIdAsync(ownerId);
            if (company == null)
            {
                throw new Exception("Company not found for this owner");
            }

            VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

            var requestData = new VenueCreateRequestDataDto
            {
                Name = dto.Name,
                Description = dto.Description,
                City = dto.City,
                Address = dto.Address,
                Capacity = dto.Capacity,
                Type = dto.Type,
                Category = dto.Category,
                PricingType = dto.PricingType,
                PricePerHour = dto.PricePerHour,
                DepositPercentage = dto.DepositPercentage,
                FacebookUrl = dto.FacebookUrl,
                InstagramUrl = dto.InstagramUrl,
                WebsiteUrl = dto.WebsiteUrl,
                TimeSlots = dto.TimeSlots,
                ImageUrls = dto.ImageUrls,
                CompanyName = company.Name
            };

            var request = new EditRequest(
                ownerId,
                EditRequestTypeEnum.VenueCreate,
                null,
                JsonSerializer.Serialize(requestData, RequestJsonOptions));

            await _editRequestRepo.AddAsync(request);
            await _editRequestRepo.SaveChangesAsync();
        }

        public async Task<List<EditRequestDto>> GetMyRequestsAsync(int ownerId)
        {
            var requests = await _editRequestRepo.GetByOwnerIdAsync(ownerId);
            return requests.Select(MapRequest).ToList();
        }

        public async Task<List<EditRequestDto>> GetAllRequestsAsync()
        {
            var requests = await _editRequestRepo.GetAllAsync();
            return requests.Select(MapRequest).ToList();
        }

        public async Task ApproveAsync(int requestId, int adminId)
        {
            var request = await _editRequestRepo.GetByIdAsync(requestId);
            if (request == null)
            {
                throw new Exception("Edit request not found");
            }

            if (request.Status != EditRequestStatusEnum.Pending)
            {
                throw new Exception("Already processed");
            }

            switch (request.Type)
            {
                case EditRequestTypeEnum.Profile:
                    await ApproveProfileRequestAsync(request);
                    break;

                case EditRequestTypeEnum.Venue:
                case EditRequestTypeEnum.VenueUpdate:
                    await ApproveVenueRequestAsync(request);
                    break;

                case EditRequestTypeEnum.VenueCreate:
                    await ApproveVenueCreateRequestAsync(request);
                    break;

                default:
                    throw new Exception("Unsupported edit request type");
            }

            request.Approve(adminId);
            await _editRequestRepo.SaveChangesAsync();

            var owner = await _userRepo.GetUserByIdAsync(request.OwnerId);
            if (owner != null)
            {
                await _emailService.SendEmailAsync(
                    owner.Email,
                    "Your Request Has Been Approved",
                    $@"
<p>Dear {owner.FirstName},</p>
<p>Your <strong>{request.Type}</strong> request has been approved.</p>
<p>Best regards,<br/>Events Team</p>");
            }
        }

        public async Task RejectAsync(int requestId, int adminId, string? reason)
        {
            var request = await _editRequestRepo.GetByIdAsync(requestId);
            if (request == null)
            {
                throw new Exception("Edit request not found");
            }

            if (request.Status != EditRequestStatusEnum.Pending)
            {
                throw new Exception("Already processed");
            }

            var normalizedReason = RejectReasonHelper.Normalize(reason);

            request.Reject(adminId, normalizedReason);
            await _editRequestRepo.SaveChangesAsync();

            var owner = await _userRepo.GetUserByIdAsync(request.OwnerId);
            if (owner != null)
            {
                await _emailService.SendEmailAsync(
                    owner.Email,
                    "Your Request Has Been Rejected",
                    $@"
<p>Dear {owner.FirstName},</p>
<p>Your <strong>{request.Type}</strong> request has been rejected.</p>
<p><strong>Reason:</strong> {normalizedReason}</p>
<p>Best regards,<br/>Events Team</p>");
            }
        }

        private async Task ApproveProfileRequestAsync(EditRequest request)
        {
            var owner = await _userRepo.GetUserByIdAsync(request.OwnerId);
            if (owner == null)
            {
                throw new Exception("Owner not found");
            }

            var dto = JsonSerializer.Deserialize<ProfileEditRequestDto>(request.RequestedDataJson, RequestJsonOptions)
                ?? throw new Exception("Invalid request data");

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingUser = await _userRepo.GetUserByEmailAsync(dto.Email);
                if (existingUser != null && existingUser.Id != owner.Id)
                {
                    throw new Exception("Email already in use by another account");
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.FirstName) || !string.IsNullOrWhiteSpace(dto.LastName))
            {
                owner.UpdateName(
                    string.IsNullOrWhiteSpace(dto.FirstName) ? owner.FirstName : dto.FirstName,
                    string.IsNullOrWhiteSpace(dto.LastName) ? owner.LastName : dto.LastName);
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) || !string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                owner.UpdateContactInfo(
                    string.IsNullOrWhiteSpace(dto.Email) ? owner.Email : dto.Email,
                    string.IsNullOrWhiteSpace(dto.PhoneNumber) ? owner.PhoneNumber : dto.PhoneNumber);
            }

            await _userRepo.UpdateUserAsync();
        }

        private async Task ApproveVenueRequestAsync(EditRequest request)
        {
            if (!request.TargetId.HasValue)
            {
                throw new Exception("Venue target not found");
            }

            var venue = await _venueRepo.GetByIdAsync(request.TargetId.Value);
            if (venue == null)
            {
                throw new Exception("Venue not found");
            }

            var wrappedDto = JsonSerializer.Deserialize<VenueEditRequestDataDto>(request.RequestedDataJson, RequestJsonOptions);
            var dto = wrappedDto?.Requested
                ?? JsonSerializer.Deserialize<VenueEditRequestDto>(request.RequestedDataJson, RequestJsonOptions)
                ?? throw new Exception("Invalid request data");

            VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

            venue.Update(
                dto.Name,
                dto.Description,
                dto.City,
                dto.Address,
                dto.Capacity,
                dto.IsActive,
                dto.Type,
                dto.Category,
                dto.PricingType,
                dto.PricePerHour,
                dto.DepositPercentage,
                dto.FacebookUrl,
                dto.InstagramUrl,
                dto.WebsiteUrl);

            if (dto.TimeSlots != null)
            {
                VenueSlotSupport.SyncSlots(venue, dto.TimeSlots);
            }

            await _venueRepo.UpdateAsync(venue);
        }

        private async Task ApproveVenueCreateRequestAsync(EditRequest request)
        {
            var company = await _companyRepo.GetByUserIdAsync(request.OwnerId);
            if (company == null)
            {
                throw new Exception("Company not found for this owner");
            }

            var dto = JsonSerializer.Deserialize<VenueCreateRequestDataDto>(request.RequestedDataJson, RequestJsonOptions)
                ?? throw new Exception("Invalid request data");

            VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

            var venue = new Venue(
                dto.Name,
                dto.Description,
                dto.City,
                dto.Address,
                dto.Capacity,
                company.Id,
                dto.Type,
                dto.Category,
                dto.PricingType,
                dto.PricePerHour,
                dto.DepositPercentage,
                dto.FacebookUrl,
                dto.InstagramUrl,
                dto.WebsiteUrl);

            if (dto.ImageUrls.Count > 0)
            {
                venue.AddImages(dto.ImageUrls);
            }

            if (dto.TimeSlots != null)
            {
                VenueSlotSupport.SyncSlots(venue, dto.TimeSlots);
            }

            await _venueRepo.AddAsync(venue);
        }

        private static EditRequestDto MapRequest(EditRequest request)
        {
            return new EditRequestDto
            {
                Id = request.Id,
                OwnerId = request.OwnerId,
                OwnerName = request.Owner?.FullName ?? string.Empty,
                Type = request.Type.ToString(),
                Status = request.Status.ToString(),
                TargetId = request.TargetId,
                RequestedDataJson = request.RequestedDataJson,
                CreatedAt = request.CreatedAt,
                ReviewedByAdminId = request.ReviewedByAdminId,
                ReviewedAt = request.ReviewedAt,
                RejectionReason = request.RejectionReason
            };
        }
    }
}
