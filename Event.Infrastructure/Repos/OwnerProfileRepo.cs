using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;

namespace Event.Infrastructure.Repos
{
    public class OwnerProfileRepo : IOwnerProfileRepo
    {
        private readonly ApplicationDbContext _db;

        public OwnerProfileRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(OwnerProfile ownerProfile)
        {
            await _db.OwnerProfiles.AddAsync(ownerProfile);
            await _db.SaveChangesAsync();
        }
    }
}