using events.domain.Entites;
namespace events.domain.Entities
{
    public class User : BaseEntity
    {
        public User() { }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; } 
        public string PhoneNumber { get; private set; }
        public string FirstName { get; private set; } 
        public string LastName { get; private set; }
        public string? SecondaryPhoneNumber { get; private set; }
        public string FullName => $"{FirstName} {LastName}";
        public DateTime? LastLoginAt { get; private set; }
        public bool IsActive { get; private set; } 
        public int RoleId { get; private set; }
        public UserRole Role { get; private set; }
        public Company? Company { get; private set; }    
        public List<Booking> Bookings { get; private set; } = new List<Booking>();
        public List<Review> Reviews { get; private set; } = new List<Review>();


        public User(string email, string passwordHash, string phoneNumber,
                    string firstName, string lastName,  int roleId)
        {
            Email = email;
            PasswordHash = passwordHash;
            PhoneNumber = phoneNumber;
            FirstName = firstName;
            LastName = lastName;
            IsActive = true;
            RoleId = roleId;
        }
        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public void UpdateName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            UpdatedAt = DateTime.UtcNow;
        }
        public void UpdateContactInfo(string email, string phoneNumber)
        {
            Email = email;
            PhoneNumber = phoneNumber;
            UpdatedAt = DateTime.UtcNow;
        }

        public void changePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }
        public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
        public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    }
}