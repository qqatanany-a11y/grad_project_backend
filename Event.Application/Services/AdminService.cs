using Event.Application.Dtos;
using Event.Application.Helpers;
using Event.Application.IServices;
using events.domain.Entites;
using events.domain.Entities;
using events.domain.Repos;
using System.Text.Json;

namespace Event.Application.Services
{
    public class AdminService : IAdminService
    {
        private static readonly JsonSerializerOptions RequestJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IOwnerRequestRepo _ownerRequestRepo;
        private readonly IUserRepo _userRepo;
        private readonly ICompanyRepo _companyRepo;
        private readonly IVenueRepo _venueRepo;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly IEmailService _emailService;
        private readonly IEditRequestRepo _editRequestRepo;

        public AdminService(
            IOwnerRequestRepo ownerRequestRepo,
            IUserRepo userRepo,
            ICompanyRepo companyRepo,
            IVenueRepo venueRepo,
            IPasswordGenerator passwordGenerator,
            IEmailService emailService,
            IEditRequestRepo editRequestRepo)
        {
            _ownerRequestRepo = ownerRequestRepo;
            _userRepo = userRepo;
            _companyRepo = companyRepo;
            _venueRepo = venueRepo;
            _passwordGenerator = passwordGenerator;
            _emailService = emailService;
            _editRequestRepo = editRequestRepo;
        }

        public async Task<List<OwnerRequest>> GetOwnerRequestsAsync()
        {
            return await _ownerRequestRepo.GetAllAsync();
        }

        public async Task ApproveOwnerAsync(int requestId)
        {
            var request = await _ownerRequestRepo.GetByIdAsync(requestId);
            if (request == null)
            {
                throw new Exception("Request not found");
            }

            if (!string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Already processed");
            }

            var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new Exception("User already exists");
            }

            var rawPassword = _passwordGenerator.Generate(10);
            var ownerUser = new User(
                request.Email,
                BCrypt.Net.BCrypt.HashPassword(rawPassword),
                request.PhoneNumber,
                request.FirstName,
                request.LastName,
                3);

            await _userRepo.AddUserAsync(ownerUser);

            var company = new Company(
                request.CompanyName,
                request.BusinessAddress,
                request.BusinessPhone,
                request.Email,
                ownerUser.Id);

            await _companyRepo.AddAsync(company);

            request.Approve();
            await _ownerRequestRepo.UpdateAsync(request);

            await _emailService.SendEmailAsync(
                request.Email,
                "Owner Account Approved",
                $@"
<h2>Welcome to Events Platform</h2>
<p>Dear {request.FirstName},</p>
<p>Your owner request has been approved.</p>
<p><strong>Email:</strong> {request.Email}</p>
<p><strong>Temporary password:</strong> {rawPassword}</p>
<p>Please login and change your password immediately.</p>
<p>Best regards,<br/>Events Team</p>");
        }

        public async Task RejectOwnerAsync(int id, string? reason)
        {
            var request = await _ownerRequestRepo.GetByIdAsync(id);
            if (request == null)
            {
                throw new Exception("Request not found");
            }

            var normalizedReason = RejectReasonHelper.Normalize(reason);

            request.Reject(normalizedReason);
            await _ownerRequestRepo.UpdateAsync(request);

            await _emailService.SendEmailAsync(
                request.Email,
                "Owner Account Request Update",
                $@"
<p>Dear {request.FirstName},</p>
<p>Your owner request has been rejected.</p>
<p><strong>Reason:</strong> {normalizedReason}</p>
<p>Best regards,<br/>Events Team</p>");
        }

        public async Task OwnerRequestAsync(RegisterOwnerDto dto)
        {
            var existingUser = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists");
            }

            var request = new OwnerRequest(
                dto.Email,
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                dto.CompanyName,
                dto.BusinessAddress,
                dto.BusinessPhone);

            await _ownerRequestRepo.AddAsync(request);

            await _emailService.SendEmailAsync(
                dto.Email,
                "Owner Account Request Received",
                $@"
<p>Dear {dto.FirstName},</p>
<p>Your owner request has been received and is pending review.</p>
<p>Best regards,<br/>Events Team</p>");
        }

        public async Task ApproveVenueUpdate(int requestId, int adminId)
        {
            var request = await _editRequestRepo.GetByIdAsync(requestId);
            if (request == null)
            {
                throw new Exception("Request not found");
            }

            if (request.Type != EditRequestTypeEnum.VenueUpdate && request.Type != EditRequestTypeEnum.Venue)
            {
                throw new Exception("Invalid request type");
            }

            if (!request.TargetId.HasValue)
            {
                throw new Exception("Venue target not found");
            }

            var venue = await _venueRepo.GetByIdAsync(request.TargetId.Value);
            if (venue == null)
            {
                throw new Exception("Venue not found");
            }

            var wrappedData = JsonSerializer.Deserialize<VenueEditRequestDataDto>(request.RequestedDataJson, RequestJsonOptions);
            var data = wrappedData?.Requested
                ?? JsonSerializer.Deserialize<VenueEditRequestDto>(request.RequestedDataJson, RequestJsonOptions)
                ?? throw new Exception("Invalid request data");

            data.PricingType = PricingType.FixedSlots;
            data.PricePerHour = null;

            VenueSlotSupport.ValidateVenuePricing(data.PricingType, data.PricePerHour, data.TimeSlots);

            venue.Update(
                data.Name,
                data.Description,
                data.City,
                data.Address,
                data.Capacity,
                data.IsActive,
                data.Type,
                data.Category,
                data.PricingType,
                data.PricePerHour,
                data.DepositPercentage,
                data.FacebookUrl,
                data.InstagramUrl,
                data.WebsiteUrl);

            if (data.TimeSlots != null)
            {
                VenueSlotSupport.SyncSlots(venue, data.TimeSlots);
            }

            request.Approve(adminId);

            await _venueRepo.UpdateAsync(venue);
            await _editRequestRepo.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                venue.Company.Email,
                "Venue Update Approved",
                $@"
<p>Dear {venue.Company.Name},</p>
<p>Your venue update request has been approved.</p>
<p>Best regards,<br/>Events Team</p>");
        }

        public async Task RejectVenueUpdate(int requestId, int adminId, string? reason)
        {
            var request = await _editRequestRepo.GetByIdAsync(requestId);
            if (request == null)
            {
                throw new Exception("Request not found");
            }

            var normalizedReason = RejectReasonHelper.Normalize(reason);

            request.Reject(adminId, normalizedReason);
            await _editRequestRepo.SaveChangesAsync();

            var owner = await _userRepo.GetUserByIdAsync(request.OwnerId);
            var recipient = owner?.Email;

            if (string.IsNullOrWhiteSpace(recipient) && request.TargetId.HasValue)
            {
                var venue = await _venueRepo.GetByIdAsync(request.TargetId.Value);
                recipient = venue?.Company.Email;
            }

            if (!string.IsNullOrWhiteSpace(recipient))
            {
                await _emailService.SendEmailAsync(
                    recipient,
                    "Venue Update Rejected",
                    $@"
<p>Your venue update request has been rejected.</p>
<p><strong>Reason:</strong> {normalizedReason}</p>
<p>Best regards,<br/>Events Team</p>");
            }
        }

        public async Task<List<Company>> GetCompaniesAsync()
        {
            return await _companyRepo.GetAllAsync();
        }

        public async Task<List<Venue>> GetVenuesAsync()
        {
            return await _venueRepo.GetAllAsync();
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _userRepo.GetAllUsersAsync();
        }
    }
}
