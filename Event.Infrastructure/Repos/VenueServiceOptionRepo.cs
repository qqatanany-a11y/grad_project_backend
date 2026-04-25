using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class VenueServiceOptionRepo : IVenueServiceOptionRepo
    {
        private readonly ApplicationDbContext _db;

        public VenueServiceOptionRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<VenueServiceOption>> GetByVenueIdAsync(int venueId)
        {
            return await _db.VenueServiceOptions
                .Include(x => x.Service)
                .Where(x => x.VenueId == venueId && x.IsActive)
                .ToListAsync();
        }

        public async Task<List<VenueServiceOption>> GetByIdsAsync(List<int> ids)
        {
            return await _db.VenueServiceOptions
                .Where(x => ids.Contains(x.Id) && x.IsActive)
                .ToListAsync();
        }

        public async Task AddAsync(VenueServiceOption option)
        {
            await _db.VenueServiceOptions.AddAsync(option);
            await _db.SaveChangesAsync();
        }
    }
}