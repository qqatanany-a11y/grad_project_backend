using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IServiceRepo
    {
        Task<Service> AddAsync(Service service);
        Task<List<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(int id);
    }
}