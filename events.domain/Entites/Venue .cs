

namespace events.domain.Entities
{
    public class Venue
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public string? City { get; private set; }
        public string? Address { get; private set; }
        public int Capacity { get; private set; }
        public decimal MinimalPrice { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public int OwnerId { get; private set; }
        public User Owner { get; private set; } 


        public virtual ICollection<VenueImage> Images { get; private set; } = new List<VenueImage>();
        public virtual ICollection<VenueEventType> VenueEventTypes { get; private set; } = new List<VenueEventType>();
        public virtual ICollection<VenueAvailability> Availabilities { get; private set; } = new List<VenueAvailability>();
        public virtual ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; private set; } = new List<Review>();
    }
}
