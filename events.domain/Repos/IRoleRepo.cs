using events.domain.Entities;


namespace events.domain.Repos
{
    public interface IRoleRepo
    {
        Task<UserRole?> GetRoleByNameAsync(string name);
        Task<UserRole?> GetRoleByIdAsync(int id);


    }
}
