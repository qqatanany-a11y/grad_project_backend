using events.domain.Entites;

namespace events.domain.Entities
{
    public class Review : BaseEntity
    {
        private Review() { }

        public int VenueId { get; private set; }
        public int BookingId { get; private set; }
        public int UserId { get; private set; }

        public int Rating { get; private set; }
        public string? Comment { get; private set; }

        public User User { get; private set; }
        public Venue Venue { get; private set; }
        public Booking Booking { get; private set; }

        public Review(int venueId, int bookingId, int userId, int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
                throw new Exception("Rating must be between 1 and 5");

            VenueId = venueId;
            BookingId = bookingId;
            UserId = userId;
            Rating = rating;
            Comment = comment;
        }
    }
}