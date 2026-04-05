using events.domain.Entities;

namespace events.domain.Entites
{
    public class Company : BaseEntity
    {
        private Company() { }
        public string Name { get; private set; }
        public string Location { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Email { get; private set; }

        //  public ICollection<Venue> venues { get; private set; } = new List<Venue>();
       // public ICollection<User> Users { get; private set; } = new List<User>();
        public Company(string name, string location, string phoneNumber, string email) 
        {
            Name = name;
            Location = location;
            PhoneNumber = phoneNumber;
            Email = email;
        }
    }
}
