using events.domain.Entites;
using events.domain.Entities;

namespace events.domain.Repos
{
    public interface ICompanyRepo
    {
        Task<List<Company>> GetAllAsync();                    
        Task<Company?> GetByIdAsync(int id);
        Task<Company?> GetByUserIdAsync(int userId);
        Task AddAsync(Company company);
        Task UpdateAsync(Company company);
        Task DeleteAsync(Company company);

        Task AddCompanyAsync(Company company);
    }
}