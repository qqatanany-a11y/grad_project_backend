using events.domain.Entites;

namespace events.domain.Entities
{
    public class SystemAdmin : BaseEntity
    {
        private SystemAdmin() { }

        public string Email { get; private set; } 
        public string PasswordHash { get; private set; } 
        public string PhoneNumber { get; private set; } 
        public string FirstName { get; private set; } 
        public string? MiddleName { get; private set; }
        public string LastName { get; private set; } 
        public bool IsActive { get; private set; } = true;

        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        public SystemAdmin(string email, string passwordHash, string phoneNumber,
                           string firstName, string lastName, string? middleName)
        {
            Email = email;
            PasswordHash = passwordHash;
            PhoneNumber = phoneNumber;
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateName(string firstName, string lastName, string? middleName)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateContact(string email, string phoneNumber)
        {
            Email = email;
            PhoneNumber = phoneNumber;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
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
