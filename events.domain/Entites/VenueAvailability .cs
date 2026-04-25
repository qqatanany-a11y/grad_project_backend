using events.domain.Entites;

namespace events.domain.Entities
{
    public class VenueAvailability : BaseEntity
    {
        public int VenueId { get; private set; }
        public Venue Venue { get; private set; } = null!;

        public DateOnly Date { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public decimal Price { get; private set; }
        public bool IsBooked { get; private set; } = false;

        private VenueAvailability() { }

        public VenueAvailability(
            int venueId,
            DateOnly date,
            TimeSpan startTime,
            TimeSpan endTime,
            decimal price)
        {
            VenueId = venueId;
            Date = date;
            StartTime = startTime;
            EndTime = endTime;
            Price = price;
            IsBooked = false;
        }

        public void MarkAsBooked()
        {
            IsBooked = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsAvailable()
        {
            IsBooked = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal price)
        {
            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}