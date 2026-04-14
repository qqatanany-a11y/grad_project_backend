using events.domain.Entites;
using events.domain.Entities;

namespace Event.Application.IServices
{
    public interface IAdminService
    {
        Task<List<OwnerRequest>> GetOwnerRequestsAsync();
        Task ApproveOwnerAsync(int requestId);
        Task RejectOwnerAsync(int requestId);




        Task<List<Company>> GetCompaniesAsync();
        Task<List<Venue>> GetVenuesAsync();
        Task<List<User>> GetUsersAsync();
    }
}