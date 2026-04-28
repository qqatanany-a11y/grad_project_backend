using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IPaymentRepo
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetByBookingIdAsync(int bookingId);
        Task SaveChangesAsync();
    }
}