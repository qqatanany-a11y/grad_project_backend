using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IVenueAvailabilityService
    {
        Task<VenueAvailabilityItemDto> AddAsync(int ownerId, CreateVenueAvailabilityDto dto);
        Task<List<VenueAvailabilityItemDto>> GetByVenueIdAsync(int ownerId, int venueId);
        Task<List<VenueAvailabilityItemDto>> GetAvailableSlotsAsync(int venueId, DateOnly date);
    }
}