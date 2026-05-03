using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class PaymentRepo : IPaymentRepo
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task<Payment?> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Venue)
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}