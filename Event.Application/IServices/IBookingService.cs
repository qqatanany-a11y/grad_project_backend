using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IBookingService
    {
        Task<CreateBookingResponseDto> CreateBooking(int userId, CreateBookingDto dto);
        Task<List<BookingDto>> GetMyBookings(int userId);
        Task<List<BookingDto>> GetOwnerBookings(int ownerId);
        Task Approve(int bookingId, int ownerId);
        Task Reject(int bookingId, int ownerId, string? reason);
        Task<string> Cancel(int bookingId, int userId);


    }
}
