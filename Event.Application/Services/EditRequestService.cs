using System.Text.Json;
using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

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
            IVenueRepo venueRepo)
            IVenueRepo venueRepo,
            ICompanyRepo companyRepo)
        {
            _editRequestRepo = editRequestRepo;
            _userRepo = userRepo;
            _venueRepo = venueRepo;
            _emailService = emailService;
            _companyRepo = companyRepo;
        }

        public async Task CreateProfileEditRequestAsync(int ownerId, ProfileEditRequestDto dto)
        {
            var owner = await _userRepo.GetUserByIdAsync(ownerId);
            if (owner == null)
                throw new Exception("Owner not found");

            var json = JsonSerializer.Serialize(dto, RequestJsonOptions);

            var request = new EditRequest(
                ownerId,
                EditRequestTypeEnum.Profile,
                null,
                json
            );
            await _emailService.SendEmailAsync(
     "laithalnobane323@gmail.com", 
     "New Profile Edit Request",
     $@"
    <h2>New Profile Edit Request</h2>

    <p>An owner has submitted a profile edit request.</p>

    <p><strong>Owner Name:</strong> {owner.FirstName}</p>
    <p><strong>Email:</strong> {owner.Email}</p>

    <p>Please review the request in the admin dashboard.</p>

    <br/>

    <p>Best regards,<br/>Events System</p>
    "
 );
            await _emailService.SendEmailAsync(
    owner.Email,
    "Profile Edit Request Submitted",
    $@"
    <h2>Events Platform</h2>

    <p>Dear {owner.FirstName},</p>

    <p>Your profile edit request has been successfully submitted and is currently under review.</p>

    <p>You will be notified once it has been approved or rejected.</p>

    <br/>

    <p>Best regards,<br/>Events Team</p>
    "
);
            await _editRequestRepo.AddAsync(request);
            await _editRequestRepo.SaveChangesAsync();
        }

        public async Task CreateVenueEditRequestAsync(int ownerId, int venueId, VenueEditRequestDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);
            if (venue == null)
                throw new Exception("Venue not found");

            if (venue.Company.UserId != ownerId)
                throw new Exception("Not allowed");

            VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

            var requestData = new VenueEditRequestDataDto
            {
                VenueId = venue.Id,
                CompanyName = venue.Company?.Name,
                Current = new VenueEditRequestDto
                {
                    Name = venue.Name,
                    Description = venue.Description,
                    City = venue.City,
                    Address = venue.Address,
                    Capacity = venue.Capacity,
                    IsActive = venue.IsActive,
                    Category = venue.Category,
                    PricingType = venue.PricingType,
                    PricePerHour = venue.PricePerHour,
                    TimeSlots = VenueSlotSupport.MapEditableSlots(venue.TimeSlots)
                },
                Requested = dto
            };

            var json = JsonSerializer.Serialize(requestData, RequestJsonOptions);

            var request = new EditRequest(
                ownerId,
                EditRequestTypeEnum.Venue,
                venueId,
                json
            );

            await _editRequestRepo.AddAsync(request);
            await _editRequestRepo.SaveChangesAsync();
        }

        public async Task CreateVenueCreateRequestAsync(int ownerId, CreateVenueRequestDto dto)
        {
            var owner = await _userRepo.GetUserByIdAsync(ownerId);
            if (owner == null)
                throw new Exception("Owner not found");

            var company = await _companyRepo.GetByUserIdAsync(ownerId);
            if (company == null)
                throw new Exception("Company not found for this owner");

            VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

            var requestData = new VenueCreateRequestDataDto
            {
                Name = dto.Name,
                Description = dto.Description,
                City = dto.City,
                Address = dto.Address,
                Capacity = dto.Capacity,
                Category = dto.Category,
                PricingType = dto.PricingType,
                PricePerHour = dto.PricePerHour,
                TimeSlots = dto.TimeSlots,
                CompanyName = company.Name
            };

            var json = JsonSerializer.Serialize(requestData, RequestJsonOptions);

            var request = new EditRequest(
                ownerId,
                EditRequestTypeEnum.VenueCreate,
                null,
                json
            );

            await _editRequestRepo.AddAsync(request);
            await _editRequestRepo.SaveChangesAsync();
        }

        public async Task<List<EditRequestDto>> GetMyRequestsAsync(int ownerId)
        {
            var requests = await _editRequestRepo.GetByOwnerIdAsync(ownerId);

            return requests.Select(x => new EditRequestDto
            {
                Id = x.Id,
                OwnerId = x.OwnerId,
                OwnerName = x.Owner?.FullName,
                Type = x.Type.ToString(),
                Status = x.Status.ToString(),
                TargetId = x.TargetId,
                RequestedDataJson = x.RequestedDataJson,
                CreatedAt = x.CreatedAt,
                ReviewedByAdminId = x.ReviewedByAdminId,
                ReviewedAt = x.ReviewedAt,
                RejectionReason = x.RejectionReason
            }).ToList();
        }

        public async Task<List<EditRequestDto>> GetAllRequestsAsync()
        {
            var requests = await _editRequestRepo.GetAllAsync();

            return requests.Select(x => new EditRequestDto
            {
                Id = x.Id,
                OwnerId = x.OwnerId,
                OwnerName = x.Owner?.FullName,
                Type = x.Type.ToString(),
                Status = x.Status.ToString(),
                TargetId = x.TargetId,
                RequestedDataJson = x.RequestedDataJson,
                CreatedAt = x.CreatedAt,
                ReviewedByAdminId = x.ReviewedByAdminId,
                ReviewedAt = x.ReviewedAt,
                RejectionReason = x.RejectionReason
            }).ToList();
        }

        public async Task ApproveAsync(int requestId, int adminId)
        {
            var request = await _editRequestRepo.GetByIdAsync(requestId);
            if (request == null)
                throw new Exception("Edit request not found");

            if (request.Status != EditRequestStatusEnum.Pending)
                throw new Exception("Already processed");

            if (request.Type == EditRequestTypeEnum.Profile)
            {
                var owner = await _userRepo.GetUserByIdAsync(request.OwnerId);
                if (owner == null)
                    throw new Exception("Owner not found");


                var dto = JsonSerializer.Deserialize<ProfileEditRequestDto>(request.RequestedDataJson);
                var dto = JsonSerializer.Deserialize<ProfileEditRequestDto>(request.RequestedDataJson, RequestJsonOptions);
                if (dto == null)
                    throw new Exception("Invalid request data");

                if(await _userRepo.GetUserByEmailAsync(dto.Email) is User existingUser && existingUser.Id != owner.Id)
                    throw new Exception("Email already in use by another account");
                if(!string.IsNullOrWhiteSpace(dto.FirstName) && !string.IsNullOrWhiteSpace(dto.LastName))
                    owner.UpdateName(dto.FirstName, dto.LastName);

                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                    owner.UpdateName(dto.FirstName, owner.LastName);

                if (!string.IsNullOrWhiteSpace(dto.LastName))
                    owner.UpdateName(owner.FirstName, dto.LastName);

                if (!string.IsNullOrWhiteSpace(dto.Email) && !string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    owner.UpdateContactInfo(dto.Email, dto.PhoneNumber);

                if (!string.IsNullOrWhiteSpace(dto.Email))
                    owner.UpdateContactInfo(dto.Email, owner.PhoneNumber);

                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    owner.UpdateContactInfo(owner.Email, dto.PhoneNumber);


                if (owner != null)
                {
                    await _emailService.SendEmailAsync(
                        owner.Email,
                        "Your Request Has Been Approved 🎉",
                        $@"
        <h2>Events Platform</h2>

        <p>Dear {owner.FirstName},</p>

        <p>Your <strong>{request.Type}</strong> edit request has been <strong>approved</strong>.</p>

        <p>The changes are now live on your account.</p>

        <br/>

        <p>Best regards,<br/>Events Team</p>
        "
                    );
                }


                await _userRepo.UpdateUserAsync();
            }
            else if (request.Type == EditRequestTypeEnum.Venue)
            {
                if (!request.TargetId.HasValue)
                    throw new Exception("Venue target not found");

                var venue = await _venueRepo.GetByIdAsync(request.TargetId.Value);
                if (venue == null)
                    throw new Exception("Venue not found");

                if (venue.Company.UserId != request.OwnerId)
                    throw new Exception("Not allowed");

                var wrappedDto = JsonSerializer.Deserialize<VenueEditRequestDataDto>(request.RequestedDataJson, RequestJsonOptions);
                var dto = wrappedDto?.Requested
                    ?? JsonSerializer.Deserialize<VenueEditRequestDto>(request.RequestedDataJson, RequestJsonOptions);

                if (dto == null)
                    throw new Exception("Invalid request data");

                VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

                venue.Update(
                    dto.Name,
                    dto.Description,
                    dto.City,
                    dto.Address,
                    dto.Capacity,
                    dto.IsActive,
                    dto.Category,
                    dto.PricingType,
                    dto.PricePerHour
                );

                if (dto.TimeSlots != null)
                {
                    VenueSlotSupport.SyncSlots(venue, dto.TimeSlots);
                }

                venue.Update(
                dto.Name,
                dto.Description,
                dto.City,
                dto.Address,
                dto.Capacity,
                dto.IsActive,
                venue.Type,
                venue.PricingType,
                venue.PricePerHour,
                venue.DepositPercentage,
                dto.FacebookUrl,
                dto.InstagramUrl,
                dto.WebsiteUrl
            );
                await _venueRepo.UpdateAsync(venue);
            }
            else if (request.Type == EditRequestTypeEnum.VenueCreate)
            {
                var company = await _companyRepo.GetByUserIdAsync(request.OwnerId);
                if (company == null)
                    throw new Exception("Company not found for this owner");

                var dto = JsonSerializer.Deserialize<VenueCreateRequestDataDto>(request.RequestedDataJson, RequestJsonOptions);
                if (dto == null)
                    throw new Exception("Invalid request data");

                VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

                var venue = new Venue(
                    dto.Name,
                    dto.Description,
                    dto.City,
                    dto.Address,
                    dto.Capacity,
                    company.Id,
                    dto.Category,
                    dto.PricingType,
                    dto.PricePerHour
                );

                if (dto.TimeSlots != null)
                {
                    VenueSlotSupport.SyncSlots(venue, dto.TimeSlots);
                }

                await _venueRepo.AddAsync(venue);
            }

            request.Approve(adminId);


            await _editRequestRepo.SaveChangesAsync();
        }

        public async Task RejectAsync(int requestId, int adminId, string? reason)
        {
            var request = await _editRequestRepo.GetByIdAsync(requestId);
            if (request == null)
                throw new Exception("Edit request not found");

            if (request.Status != EditRequestStatusEnum.Pending)
                throw new Exception("Already processed");

            request.Reject(adminId, reason);
            var owner = await _userRepo.GetUserByIdAsync(request.OwnerId);

            if (owner != null)
            {
                string requestTypeText = request.Type == EditRequestTypeEnum.Profile
                    ? "profile"
                    : "venue";

                await _emailService.SendEmailAsync(
                    owner.Email,
                    "Your Request Has Been Rejected",
                    $@"
            <h2>Events Platform</h2>

            <p>Dear {owner.FirstName},</p>

            <p>We regret to inform you that your <strong>{requestTypeText}</strong> edit request has been <strong>rejected</strong>.</p>

            <p><strong>Reason:</strong> {reason ?? "No reason provided"}</p>

            <p>You can review your information and submit a new request if needed.</p>

            <br/>

            <p>Best regards,<br/>Events Team</p>
            "
                );
            }
            await _editRequestRepo.SaveChangesAsync();
        }
    }
}
