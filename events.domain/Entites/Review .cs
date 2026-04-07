using events.domain.Entites;

namespace events.domain.Entities
{
    public class Review : BaseEntity
    {
        private Review() { }

        public int VenueId { get; private set; }
        public int BookingId { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }


        public int UserId { get; private set; }
        public User User { get; private set; }

        public Venue Venue { get; private set; } 
        public Booking Booking { get; private set; } 
    }
}