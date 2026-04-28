using events.domain.Entites;

namespace events.domain.Entities
{
    public class Booking : BaseEntity
    {
        private DateTime bookingDateUtc;

        private Booking() { }

        public int VenueId { get; private set; }
        public int? EventTypeId { get; private set; }
        public int? AprovedById { get; private set; }

        public DateTime BookingDate { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public int? GuestsCount { get; private set; }
        public decimal TotalPrice { get; private set; }
        public BookingStatusEnum Status { get; private set; } = BookingStatusEnum.Pending;
        public bool ReminderSent { get; private set; } = false;

        public int UserId { get; private set; }

        public User User { get; private set; } = null!;
        public Venue Venue { get; private set; } = null!;
        public EventType? EventType { get; private set; }
        public Payment? Payment { get; private set; }
        public Review? Review { get; private set; }

        public decimal BasePrice { get; private set; }
        public decimal ServicesPrice { get; private set; }
        public string? BrideIdDocumentDataUrl { get; private set; }
        public string? BridegroomIdDocumentDataUrl { get; private set; }
        public List<BookingSelectedService> SelectedServices { get; private set; } = new();

        public Booking(
            int venueId,
            int userId,
            DateTime bookingDate,
            TimeSpan startTime,
            TimeSpan endTime,
            int? guestsCount,
            decimal basePrice,
            decimal servicesPrice,
            decimal totalPrice,
            string? brideIdDocumentDataUrl,
            string? bridegroomIdDocumentDataUrl)
        {
            VenueId = venueId;
            UserId = userId;
            BookingDate = bookingDate;
            StartTime = startTime;
            EndTime = endTime;
            GuestsCount = guestsCount;
            BasePrice = basePrice;
            ServicesPrice = servicesPrice;
            TotalPrice = totalPrice;
            BrideIdDocumentDataUrl = brideIdDocumentDataUrl;
            BridegroomIdDocumentDataUrl = bridegroomIdDocumentDataUrl;
            Status = BookingStatusEnum.Pending;
            ReminderSent = false;
        }

        public Booking(int venueId, int userId, DateTime bookingDateUtc, TimeSpan startTime, TimeSpan endTime, int? guestsCount, decimal totalPrice)
        {
            VenueId = venueId;
            UserId = userId;
            this.bookingDateUtc = bookingDateUtc;
            StartTime = startTime;
            EndTime = endTime;
            GuestsCount = guestsCount;
            TotalPrice = totalPrice;
        }

        public void Approve(int ownerId)
        {
            Status = BookingStatusEnum.Confirmed;
            AprovedById = ownerId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(int ownerId)
        {
            Status = BookingStatusEnum.Rejected;
            AprovedById = ownerId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            Status = BookingStatusEnum.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkReminderSent()
        {
            ReminderSent = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
