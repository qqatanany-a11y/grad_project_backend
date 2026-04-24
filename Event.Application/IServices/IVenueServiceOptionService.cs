using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IVenueServiceOptionService
    {
        Task<VenueServiceOptionDto> AddAsync(AddVenueServiceOptionDto dto);
        Task<List<VenueServiceOptionDto>> GetByVenueIdAsync(int venueId);
    }
}