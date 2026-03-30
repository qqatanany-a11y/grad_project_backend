
namespace events.domain.Entities
{
    public class Booking
    {
        private Booking() { } // EF Core

        // =========================
        // Primary Key
        // =========================
        public int Id { get; private set; }

        // =========================
        // Foreign Keys
        // =========================
        public int UserId { get; private set; }
        public int VenueId { get; private set; }
        public int? EventTypeId { get; private set; }

        // =========================
        // Main Fields
        // =========================
        public DateTime BookingDate { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public int? GuestsCount { get; private set; }
        public decimal TotalPrice { get; private set; }

        public BookingStatus Status { get; private set; } = BookingStatus.Pending;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        // =========================
        // Navigation Properties
        // =========================
        public User User { get; private set; }
        public Venue Venue { get; private set; } 
        public EventType? EventType { get; private set; }
        public Payment? Payment { get; private set; }
        public Review? Review { get; private set; }
    }
}