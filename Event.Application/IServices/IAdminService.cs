using Event.Application.Dtos;
using events.domain.Entites;
using events.domain.Entities;

namespace Event.Application.IServices
{
    public interface IAdminService
    {
        Task<List<OwnerRequest>> GetOwnerRequestsAsync();
        Task ApproveOwnerAsync(int requestId);
        Task RejectOwnerAsync(int id, string? reason);

        Task OwnerRequestAsync(RegisterOwnerDto dto);
        Task ApproveVenueUpdate(int requestId, int adminId);
        Task RejectVenueUpdate(int requestId, int adminId, string? reason);


        Task<List<Company>> GetCompaniesAsync();
        Task<List<Venue>> GetVenuesAsync();
        Task<List<User>> GetUsersAsync();
    }
}
