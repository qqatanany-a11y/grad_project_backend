using events.domain.Entities;

namespace events.domain.Repos
{
    public interface ISystemAdminRepo
    {
        Task<SystemAdmin?> GetByEmailAsync(string email);
        Task AddAsync(SystemAdmin admin);
    }
}