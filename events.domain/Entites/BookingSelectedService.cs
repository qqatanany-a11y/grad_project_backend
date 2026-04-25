using events.domain.Entites;

namespace events.domain.Entities
{
    public class BookingSelectedService : BaseEntity
    {
        public int BookingId { get; private set; }
        public Booking Booking { get; private set; } = null!;

        public int VenueServiceOptionId { get; private set; }
        public VenueServiceOption VenueServiceOption { get; private set; } = null!;

        public decimal Price { get; private set; }

        private BookingSelectedService() { }

        public BookingSelectedService(int bookingId, int venueServiceOptionId, decimal price)
        {
            BookingId = bookingId;
            VenueServiceOptionId = venueServiceOptionId;
            Price = price;
        }
    }
}