using System.Text.Json;
using Event.Application.Dtos;
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
        private readonly ICompanyRepo _companyRepo;

        public EditRequestService(
            IEditRequestRepo editRequestRepo,
            IUserRepo userRepo,
            IVenueRepo venueRepo,
            ICompanyRepo companyRepo)
        {
            _editRequestRepo = editRequestRepo;
            _userRepo = userRepo;
            _venueRepo = venueRepo;
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
                    PricePerHour = venue.PricePerHour
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

                var dto = JsonSerializer.Deserialize<ProfileEditRequestDto>(request.RequestedDataJson, RequestJsonOptions);
                if (dto == null)
                    throw new Exception("Invalid request data");

                owner.UpdateName(dto.FirstName, dto.LastName);
                owner.UpdateContactInfo(dto.Email, dto.PhoneNumber);

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
            await _editRequestRepo.SaveChangesAsync();
        }
    }
}
