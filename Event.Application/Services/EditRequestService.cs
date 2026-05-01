using System.Text.Json;
using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class EditRequestService : IEditRequestService
    {
        private readonly IEditRequestRepo _editRequestRepo;
        private readonly IUserRepo _userRepo;
        private readonly IVenueRepo _venueRepo;

        public EditRequestService(
            IEditRequestRepo editRequestRepo,
            IUserRepo userRepo,
            IVenueRepo venueRepo)
        {
            _editRequestRepo = editRequestRepo;
            _userRepo = userRepo;
            _venueRepo = venueRepo;
        }

        public async Task CreateProfileEditRequestAsync(int ownerId, ProfileEditRequestDto dto)
        {
            var owner = await _userRepo.GetUserByIdAsync(ownerId);
            if (owner == null)
                throw new Exception("Owner not found");

            var json = JsonSerializer.Serialize(dto);

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

            var json = JsonSerializer.Serialize(dto);

            var request = new EditRequest(
                ownerId,
                EditRequestTypeEnum.Venue,
                venueId,
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

                var dto = JsonSerializer.Deserialize<VenueEditRequestDto>(request.RequestedDataJson);
                if (dto == null)
                    throw new Exception("Invalid request data");



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