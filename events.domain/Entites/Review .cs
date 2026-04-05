using events.domain.Entites;

namespace events.domain.Entities
{
    public class Review : BaseEntity
    {
        private Review() { }

        public int ClientId { get; private set; }
        public int VenueId { get; private set; }
        public int BookingId { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }

        public Client Client { get; private set; } 
        public Venue Venue { get; private set; } 
        public Booking Booking { get; private set; } 
    }
}