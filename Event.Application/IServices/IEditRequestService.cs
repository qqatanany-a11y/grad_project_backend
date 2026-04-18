using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IEditRequestService
    {
        Task CreateProfileEditRequestAsync(int ownerId, ProfileEditRequestDto dto);
        Task CreateVenueEditRequestAsync(int ownerId, int venueId, VenueEditRequestDto dto);

        Task<List<EditRequestDto>> GetMyRequestsAsync(int ownerId);
        Task<List<EditRequestDto>> GetAllRequestsAsync();

        Task ApproveAsync(int requestId, int adminId);
        Task RejectAsync(int requestId, int adminId, string? reason);
    }
}