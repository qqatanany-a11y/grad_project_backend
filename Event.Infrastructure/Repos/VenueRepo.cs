using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class VenueRepo : IVenueRepo
    {
        private readonly ApplicationDbContext _db;

        public VenueRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Venue>> GetByCompanyIdAsync(int companyId)
        {
           
            return await _db.Venues
                .Where(v => v.CompanyId == companyId)
                .Include(v => v.Company) 
                .ToListAsync();
        }
        public async Task<Venue?> GetByIdAsync(int id)
        {
            return await _db.Venues
                .Include(v => v.Company)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AddAsync(Venue venue)
        {
            await _db.Venues.AddAsync(venue);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Venue venue)
        {
            _db.Venues.Update(venue);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Venue venue)
        {
            _db.Venues.Remove(venue);
            await _db.SaveChangesAsync();
        }
    }
}