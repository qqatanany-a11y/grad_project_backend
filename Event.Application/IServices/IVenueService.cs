using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IVenueService
    {
        Task<List<VenueDto>> GetByCompanyIdAsync(int companyId);
        Task<VenueDto> AddAsync(AddVenueDto dto, int companyId);
        Task<VenueDto> UpdateAsync(int venueId, UpdateVenueDto dto, int companyId);
        Task DeleteAsync(int venueId, int companyId);
    }
}