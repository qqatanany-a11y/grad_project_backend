using events.domain.Entities;
namespace events.domain.Entites
{
    public class Company : BaseEntity
    {
        private Company() {
        }

        public string Name { get; private set; }
        public string Location { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Email { get; private set; }
        public int UserId { get; private set; }          // ← جديد: مين الـ Owner
        public User User { get; private set; }           // ← Navigation Property

        public ICollection<Venue> Venues { get; private set; } = new List<Venue>();

        public Company(string name, string location, string phoneNumber, string email, int userId)
        {
            Name = name;
            Location = location;
            PhoneNumber = phoneNumber;
            Email = email;
            UserId = userId;                             // ← جديد
        }
    }
}