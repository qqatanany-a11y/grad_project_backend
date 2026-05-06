using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IVenueService
    {
        Task<List<VenueDto>> GetByCompanyIdAsync(int companyId);
        Task<VenueDto> AddAsync(int companyId, AddVenueDto dto);
        Task<string> UpdateAsync(int ownerId, int venueId, UpdateVenueDto dto);
        Task DeleteAsync(int venueId);
        Task<List<VenueDto>> GetAllAsync();
        Task<VenueDto> GetByIdAsync(int id);
        Task<List<VenueDto>> GetByOwnerIdAsync(int OwnerId);
        Task<List<VenueDto>> SearchAsync(VenueQueryParams query);
        Task<List<VenueDto>> GetVenuesForGuestAsync();
    }
}