using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IBookingSelectedServiceRepo
    {
        Task AddRangeAsync(List<BookingSelectedService> services);
    }
}