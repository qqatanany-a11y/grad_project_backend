
namespace events.domain.Entities
{
    public class UserRole
    {
        private UserRole() { }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string[] Permation { get; private set; }

        public UserRole(string name, string[] permation) 
        {
            Name = name;
            Permation = permation;
        }

        public void UpdatePermation(string[] permation) 
        {
            Permation = permation;
        }
    }
}
