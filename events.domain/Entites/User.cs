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
        public string? MiddleName { get; private set; }
        public string LastName { get; private set; }
        public string? SecondaryPhoneNumber { get; private set; }
        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";
        public bool IsActive { get; private set; } = true;
        public int RoleId { get; private set; }
        public UserRole Role { get; private set; }
        public Company? Company { get; private set; }    // ← جديد: nullable لأن User العادي مالوش شركة
        public List<Booking> Bookings { get; private set; } = new List<Booking>();
        public List<Review> Reviews { get; private set; } = new List<Review>();


        public User(string email, string passwordHash, string phoneNumber,
                    string firstName, string lastName, string? middleName,  int roleId)
        {
            Email = email;
            PasswordHash = passwordHash;
            PhoneNumber = phoneNumber;
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            
            IsActive = true;
            RoleId = roleId;
        }

        public void UpdateName(string firstName, string lastName, string? middleName)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            UpdatedAt = DateTime.UtcNow;
        }
        public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
        public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    }
}