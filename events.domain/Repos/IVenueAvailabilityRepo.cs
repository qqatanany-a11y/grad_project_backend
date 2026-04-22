using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IVenueAvailabilityRepo
    {
        Task<VenueAvailability?> GetSlotAsync(int venueId, DateOnly date, TimeSpan startTime, TimeSpan endTime);
        Task<List<VenueAvailability>> GetByVenueIdAsync(int venueId);
        Task<List<VenueAvailability>> GetAvailableByVenueAndDateAsync(int venueId, DateOnly date);
        Task AddAsync(VenueAvailability venueAvailability);
        Task SaveChangesAsync();
    }
}