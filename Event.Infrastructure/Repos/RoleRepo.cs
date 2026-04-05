using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class RoleRepo : IRoleRepo
    {
        private readonly ApplicationDbContext _db;

        public RoleRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserRole?> GetRoleByNameAsync(string name)
        {
            return await _db.UserRoles
                .FirstOrDefaultAsync(r => r.Name == name);
        }
        public async Task<UserRole?> GetRoleByIdAsync(int id)
        {
            return await _db.UserRoles.FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}