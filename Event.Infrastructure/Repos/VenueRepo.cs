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
                .Include(v => v.Images)
                .Include(v => v.TimeSlots)
                .ToListAsync();
        }

        public async Task<List<Venue>> GetAllAsync()
        {
            return await _db.Venues
                .Include(v => v.Company)
                .Include(v => v.Images)
                .Include(v => v.TimeSlots)
                .ToListAsync();
        }

        public async Task<Venue?> GetByIdAsync(int id)
        {
            return await _db.Venues
                .Include(v => v.Company)
                .Include(v => v.Images)
                .Include(v => v.TimeSlots)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<Venue>> GetByOwnerId(int ownerId)
        {
            return await _db.Venues
                .Where(v => v.Company != null && v.Company.UserId == ownerId)
                .Include(v => v.Company)
                .Include(v => v.Images)
                .Include(v => v.TimeSlots)
                .ToListAsync();
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

        public async Task<List<Venue>> GetVenuesByCompanyIdAsync(int companyId)
        {
            return await _db.Venues
                .Where(v => v.CompanyId == companyId)
                .Include(v => v.Availabilities)
                .Include(v => v.TimeSlots)
                .Include(v => v.Images)
                .Include(v => v.VenueEventTypes)
                .ToListAsync();
        }

        public async Task<List<Venue>> GetAllActiveAsync()
        {
            return await _db.Venues
                .Where(v => v.IsActive)
                .Include(v => v.Company)
                .Include(v => v.Images)
                .Include(v => v.TimeSlots)
                .ToListAsync();
        }
    }
}
