using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repos
{
    public class EditRequestRepo : IEditRequestRepo
    {
        private readonly ApplicationDbContext _context;

        public EditRequestRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EditRequest request)
        {
            await _context.EditRequests.AddAsync(request);
        }

        public async Task<EditRequest?> GetByIdAsync(int id)
        {
            return await _context.EditRequests
                .Include(x => x.Owner)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<EditRequest>> GetAllAsync()
        {
            return await _context.EditRequests
                .Include(x => x.Owner)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<EditRequest>> GetByOwnerIdAsync(int ownerId)
        {
            return await _context.EditRequests
                .Include(x => x.Owner)
                .Where(x => x.OwnerId == ownerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}