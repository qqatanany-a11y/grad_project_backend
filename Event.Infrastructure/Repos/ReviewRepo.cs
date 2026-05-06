using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class ReviewRepo : IReviewRepo
    {
        private readonly ApplicationDbContext _db;

        public ReviewRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Review review)
        {
            await _db.Reviews.AddAsync(review);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsForBooking(int bookingId)
        {
            return await _db.Reviews.AnyAsync(r => r.BookingId == bookingId);
        }

        public async Task<List<Review>> GetByVenueIdAsync(int venueId)
        {
            return await _db.Reviews
                .Include(r => r.User)
                .Where(r => r.VenueId == venueId)
                .ToListAsync();
        }
    }
}