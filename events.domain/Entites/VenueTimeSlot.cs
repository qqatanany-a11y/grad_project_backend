using events.domain.Entites;

namespace events.domain.Entities
{
    public class VenueTimeSlot : BaseEntity
    {
        public int VenueId { get; private set; }
        public Venue Venue { get; private set; } = null!;
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public decimal Price { get; private set; }
        public bool IsActive { get; private set; } = true;

        private VenueTimeSlot() { }

        public VenueTimeSlot(
            TimeSpan startTime,
            TimeSpan endTime,
            decimal price,
            bool isActive = true)
        {
            StartTime = startTime;
            EndTime = endTime;
            Price = price;
            IsActive = isActive;
        }

        public void Update(
            TimeSpan startTime,
            TimeSpan endTime,
            decimal price,
            bool isActive)
        {
            StartTime = startTime;
            EndTime = endTime;
            Price = price;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
