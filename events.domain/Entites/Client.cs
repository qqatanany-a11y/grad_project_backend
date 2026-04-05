using events.domain.Entites;

namespace events.domain.Entities
{
    public class Client : BaseEntity
    {
        private Client() 
        { 
        }

        public string FirstName { get; private set; } 
        public string? MiddleName { get; private set; }
        public string LastName { get; private set; } 
        public string Email { get; private set; } 
        public string PasswordHash { get; private set; } 
        public string PhoneNumber { get; private set; } 
        public bool IsActive { get; private set; } = true;

        public string FullName => string.IsNullOrWhiteSpace(MiddleName)? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        public List<Booking> Bookings { get; private set; } = new List<Booking>();
        public List<Review> Reviews { get; private set; } = new List<Review>();

        public Client(string firstName, string lastName, string email,
                      string passwordHash, string phoneNumber, string? middleName = null)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;
            PhoneNumber = phoneNumber;
            MiddleName = middleName;
            IsActive = true;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}