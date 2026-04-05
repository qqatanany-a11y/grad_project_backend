using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IVenueRepo
    {
        Task<List<Venue>> GetByCompanyIdAsync(int companyId);
        Task<Venue?> GetByIdAsync(int id);
        Task AddAsync(Venue venue);
        Task UpdateAsync(Venue venue);
        Task DeleteAsync(Venue venue);
    }
}