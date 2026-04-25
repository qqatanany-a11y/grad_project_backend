using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class ServiceRepo : IServiceRepo
    {
        private readonly ApplicationDbContext _db;

        public ServiceRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Service> AddAsync(Service service)
        {
            await _db.Services.AddAsync(service);
            await _db.SaveChangesAsync();
            return service;
        }

        public async Task<List<Service>> GetAllAsync()
        {
            return await _db.Services.ToListAsync();
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _db.Services.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}