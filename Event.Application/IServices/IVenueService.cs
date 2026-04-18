using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IVenueService
    {
        Task<List<VenueDto>> GetByCompanyIdAsync(int companyId);
        Task<VenueDto> AddAsync(AddVenueDto dto);
        Task<VenueDto> UpdateAsync(int venueId, UpdateVenueDto dto);
        Task DeleteAsync(int venueId);
        Task<List<VenueDto>> GetAllAsync();
        Task<VenueDto> GetByIdAsync(int id);
        Task<List<VenueDto>> GetByOwnerIdAsync(int OwnerId);

        Task<List<VenueDto>> GetVenuesForGuestAsync();
    }
}