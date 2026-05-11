using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class BookingRepo : IBookingRepo
    {
        private readonly ApplicationDbContext _context;

        public BookingRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public async Task<List<Booking>> GetByVenueAndDate(int venueId, DateTime date)
        {
            return await _context.Bookings
                .Where(b => b.VenueId == venueId && b.BookingDate.Date == date.Date)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetUserBookings(int userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Venue)
                .Include(b => b.Payment)
                .Include(b => b.SelectedServices)
                .ThenInclude(bs => bs.VenueServiceOption)
                .ThenInclude(vso => vso.Service)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetOwnerBookings(int ownerId)
        {
            return await _context.Bookings
                .Include(b => b.Venue)
                .ThenInclude(v => v.Company)
                .Include(b => b.Payment)
                .Include(b => b.SelectedServices)
                .ThenInclude(bs => bs.VenueServiceOption)
                .ThenInclude(vso => vso.Service)
                .Where(b => b.Venue.Company.UserId == ownerId)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Venue)
                .ThenInclude(v => v.Company)
                .Include(b => b.User)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Booking>> GetBookingsForReminderAsync(DateTime fromUtc, DateTime toUtc)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Venue)
                .Where(b =>
                    b.Status == BookingStatusEnum.Confirmed &&
                    !b.ReminderSent &&
                    b.BookingDate >= fromUtc &&
                    b.BookingDate <= toUtc)
                .ToListAsync();
        }
        
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
