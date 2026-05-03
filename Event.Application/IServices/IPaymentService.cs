using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IPaymentService
    {
        Task<string> PayAsync(int userId, PayBookingDto dto);
    }
}