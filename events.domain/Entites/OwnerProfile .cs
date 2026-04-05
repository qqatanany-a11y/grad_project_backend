using events.domain.Entites;

namespace events.domain.Entities
{
    public class OwnerProfile : BaseEntity
    {
        private OwnerProfile() { }

        public int UserId { get; private set; } 
        public string CompanyName { get; private set; } = null!;
        public string BusinessPhone { get; private set; } = null!;
        public string? BusinessAddress { get; private set; }
        public User User { get; private set; } = null!;

        public OwnerProfile(int userId, string companyName, string businessPhone, string? businessAddress = null)
        {
            UserId = userId;
            CompanyName = companyName;
            BusinessPhone = businessPhone;
            BusinessAddress = businessAddress;
        }

        public void Update(string companyName, string businessPhone, string? businessAddress = null)
        {
            CompanyName = companyName;
            BusinessPhone = businessPhone;
            BusinessAddress = businessAddress;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}