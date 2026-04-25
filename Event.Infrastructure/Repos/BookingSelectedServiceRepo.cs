using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;

namespace Event.Infrastructure.Repos
{
    public class BookingSelectedServiceRepo : IBookingSelectedServiceRepo
    {
        private readonly ApplicationDbContext _db;

        public BookingSelectedServiceRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddRangeAsync(List<BookingSelectedService> services)
        {
            await _db.BookingSelectedServices.AddRangeAsync(services);
            await _db.SaveChangesAsync();
        }
    }
}