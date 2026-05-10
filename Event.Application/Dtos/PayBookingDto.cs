using events.domain.Entities;

namespace Event.Application.Dtos
{
    public class PayBookingDto
    {
        public int BookingId { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public string? CliqTransferImageDataUrl { get; set; }
    }
}
