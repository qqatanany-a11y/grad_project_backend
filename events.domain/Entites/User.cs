
using events.domain.Entites;

namespace events.domain.Entities
{
    public class User : BaseEntity
    {
        private User()
        {
        }
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string PhoneNumber { get; private set; } = null!;
        public string FullName { get; private set; }
        public bool IsActive { get; private set; } = true;

        public Guid RoleId { get; private set; }
        public UserRole Role { get; private set; }

        public  List<Venue> Venues { get; private set; } = new List<Venue>();
        public  List<Booking> Bookings { get; private set; } = new List<Booking>();
        public  List<Review> Reviews { get; private set; } = new List<Review>();

        public User(string email,string passwordHash,string phoneNumber,string fullName)
        {
            Email = email;
            PasswordHash = passwordHash;
            PhoneNumber = phoneNumber;
            FullName = fullName;
            IsActive = true;
        }
        public void UpdateName(string name)
        {
            FullName = name;
            this.UpdatedAt = DateTime.UtcNow;
        }
        public void Active()
        {
            IsActive = true;
            this.UpdatedAt = DateTime.UtcNow;
        }

        public void Deactive()
        {
            IsActive = false;
            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}
