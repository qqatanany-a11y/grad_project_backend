using events.domain.Entites;

namespace events.domain.Repos
{
    public interface ICompanyRepo
    {
        Task<Company?> GetByIdAsync(int id);
        Task<Company?> GetByUserIdAsync(int userId);  // ← للـ Owner Login
        Task AddAsync(Company company);
        Task UpdateAsync(Company company);
        Task DeleteAsync(Company company);
    }
}