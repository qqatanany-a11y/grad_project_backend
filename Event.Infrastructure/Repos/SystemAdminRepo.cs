using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class SystemAdminRepo : ISystemAdminRepo
    {
        private readonly ApplicationDbContext _db;

        public SystemAdminRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SystemAdmin?> GetByEmailAsync(string email)
        {
            return await _db.SystemAdmins
                .FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task AddAsync(SystemAdmin admin)
        {
            await _db.SystemAdmins.AddAsync(admin);
            await _db.SaveChangesAsync();
        }
    }
}