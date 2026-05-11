using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class VenueAvailabilityRepo : IVenueAvailabilityRepo
    {
        private readonly ApplicationDbContext _context;

        public VenueAvailabilityRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VenueAvailability?> GetByIdAsync(int id)
        {
            return await _context.VenueAvailabilities
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<VenueAvailability?> GetSlotAsync(
            int venueId,
            DateOnly date,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            return await _context.VenueAvailabilities
                .FirstOrDefaultAsync(x =>
                    x.VenueId == venueId &&
                    x.Date == date &&
                    x.StartTime == startTime &&
                    x.EndTime == endTime);
        }

        public async Task<List<VenueAvailability>> GetByVenueIdAsync(int venueId)
        {
            return await _context.VenueAvailabilities
                .Where(x => x.VenueId == venueId)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<List<VenueAvailability>> GetAvailableByVenueAndDateAsync(int venueId, DateOnly date)
        {
            return await _context.VenueAvailabilities
                .Where(x => x.VenueId == venueId &&
                            x.Date == date &&
                            !x.IsBooked)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task AddAsync(VenueAvailability venueAvailability)
        {
            await _context.VenueAvailabilities.AddAsync(venueAvailability);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasOverlapAsync(
     int venueId,
     DateOnly date,
     TimeSpan startTime,
     TimeSpan endTime)
        {
            return await _context.VenueAvailabilities
                .AnyAsync(x =>
                    x.VenueId == venueId &&
                    x.Date == date &&
                    startTime < x.EndTime &&
                    endTime > x.StartTime);
        }
    }
}
