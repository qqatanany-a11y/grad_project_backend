
namespace events.domain.Entities
{
    public class User
    {
        private User()
        {
        }

        public int Id { get; private set; }
        public string FullName { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public int RoleId { get; private set; }
        public UserRole Role { get; private set; }

        public virtual ICollection<Venue> Venues { get; private set; } = new List<Venue>();
        public virtual ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; private set; } = new List<Review>();

        public User(string fullName)
        {
            FullName = fullName;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }
        public void UpdateName( string name)
        {
            FullName = name;
        }
        public void Active()
        {
            IsActive = true;
        }
        public void Deactive()
        {
            IsActive = false;
        }
    }
}
