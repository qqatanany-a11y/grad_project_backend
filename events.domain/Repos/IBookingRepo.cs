using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IBookingRepo
    {
        Task AddAsync(Booking booking);
        Task<List<Booking>> GetByVenueAndDate(int venueId, DateTime date);
        Task<List<Booking>> GetUserBookings(int userId);
        Task<List<Booking>> GetOwnerBookings(int ownerId);
        Task<Booking> GetByIdAsync(int id);
        Task SaveChangesAsync();
    }
}