using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IVenueAvailabilityRepo
    {
        Task<VenueAvailability?> GetByIdAsync(int id);
        Task<VenueAvailability?> GetSlotAsync(int venueId, DateOnly date, TimeSpan startTime, TimeSpan endTime);
        Task<List<VenueAvailability>> GetByVenueIdAsync(int venueId);
        Task<List<VenueAvailability>> GetAvailableByVenueAndDateAsync(int venueId, DateOnly date);

        Task<bool> HasOverlapAsync(int venueId, DateOnly date, TimeSpan startTime, TimeSpan endTime);
        Task AddAsync(VenueAvailability venueAvailability);
        Task DeleteAsync(VenueAvailability venueAvailability);

        Task SaveChangesAsync();
    }
}
