using events.domain.Entites;
using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class CompanyRepo : ICompanyRepo
    {
        private readonly ApplicationDbContext _db;

        public CompanyRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Company?> GetByIdAsync(int id)
        {
            return await _db.Companies
                .Include(c => c.Venues)   // ← بنجيب القاعات معها
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Company?> GetByUserIdAsync(int userId)
        {
            return await _db.Companies
                .Include(c => c.Venues)
                .FirstOrDefaultAsync(c => c.UserId == userId);  // ← بنجيب شركة الـ Owner
        }

        public async Task AddAsync(Company company)
        {
            await _db.Companies.AddAsync(company);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Company company)
        {
            _db.Companies.Update(company);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Company company)
        {
            _db.Companies.Remove(company);
            await _db.SaveChangesAsync();
        }
        public async Task<List<Company>> GetAllAsync()
        {
            return await _db.Companies
                .Include(c => c.Venues)
                .ToListAsync();
        }

        public async Task AddCompanyAsync(Company company)
        {
            await _db.Companies.AddAsync(company);
            await _db.SaveChangesAsync();
        }
    }
}