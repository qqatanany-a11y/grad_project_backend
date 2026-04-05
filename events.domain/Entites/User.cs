using events.domain.Entites;
using System.ComponentModel.Design;
namespace events.domain.Entities
{
    public class User : BaseEntity
    {
        private User() { }
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string PhoneNumber { get; private set; } = null!;
        public string FirstName { get; private set; } = null!;
        public string? MiddleName { get; private set; }
        public string LastName { get; private set; } = null!;

        public int CompanyId { get; private set; }
        public Company Company { get; private set; }

        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        public bool IsActive { get; private set; } = true;
        public int RoleId { get; private set; }
        public UserRole Role { get; private set; }
        public List<Booking> Bookings { get; private set; } = new List<Booking>();
        public User(string email, string passwordHash, string phoneNumber,
                    string firstName, string lastName, string middleName, int roleId, int companyId)
        {
            Email = email;
            PasswordHash = passwordHash;
            PhoneNumber = phoneNumber;
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            IsActive = true;
            RoleId = roleId;
            CompanyId = companyId;
        }
        public void UpdateName(string firstName, string lastName, string? middleName)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            UpdatedAt = DateTime.UtcNow;
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