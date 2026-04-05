using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IClientRepo
    {
        Task<Client?> GetByEmailAsync(string email);
        Task<Client?> GetByIdAsync(int id);
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
    }
}