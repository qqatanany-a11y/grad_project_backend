using events.domain.Entites;
namespace events.domain.Entities
{
    
    public class UserRole : BaseEntity
    {
        public UserRole() { }

        public string Name { get; set; } = null!;
        public string[] Permation { get; set; } = Array.Empty<string>();

        public UserRole( string name, string[] permation)
        {
            Name = name;
            Permation = permation;
        }
        public UserRole(int id)
        {
            Id = id;    
        }

        public void UpdatePermation(string[] permation)
        {
            Permation = permation;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}