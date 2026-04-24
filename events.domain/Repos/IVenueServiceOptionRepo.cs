using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IVenueServiceOptionRepo
    {
        Task<List<VenueServiceOption>> GetByVenueIdAsync(int venueId);
        Task<List<VenueServiceOption>> GetByIdsAsync(List<int> ids);
        Task AddAsync(VenueServiceOption option);
    }
}