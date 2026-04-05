using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class ClientRepo : IClientRepo
    {
        private readonly ApplicationDbContext _db;

        public ClientRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Client?> GetByEmailAsync(string email)
        {
            return await _db.Clients
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _db.Clients
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Client client)
        {
            await _db.Clients.AddAsync(client);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            _db.Clients.Update(client);
            await _db.SaveChangesAsync();
        }
    }
}