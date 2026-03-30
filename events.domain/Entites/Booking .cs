
using events.domain.Entites;

namespace events.domain.Entities
{
    public class Booking : BaseEntity
    {
        private Booking() { } // EF Core

        public Guid UserId { get; private set; }
        public Guid VenueId { get; private set; }
        public Guid? EventTypeId { get; private set; }

    
        public DateTime BookingDate { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public int? GuestsCount { get; private set; }
        public decimal TotalPrice { get; private set; }

        public BookingStatusEnum Status { get; private set; } = BookingStatusEnum.Pending;
        
        public User User { get; private set; }
        public Venue Venue { get; private set; } 
        public EventType? EventType { get; private set; }
        public Payment? Payment { get; private set; }
        public Review? Review { get; private set; }
    }
}