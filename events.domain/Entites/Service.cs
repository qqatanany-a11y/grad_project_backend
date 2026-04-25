using events.domain.Entites;

namespace events.domain.Entities
{
    public class Service : BaseEntity
    {
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }

        public List<VenueServiceOption> VenueServices { get; private set; } = new();

        private Service() { }

        public Service(string name, string? description = null)
        {
            Name = name;
            Description = description;
        }

        public void Update(string name, string? description)
        {
            Name = name;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}