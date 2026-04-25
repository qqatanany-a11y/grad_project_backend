using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class OwnerRequestRepo : IOwnerRequestRepo
{
    private readonly ApplicationDbContext _context;

    public OwnerRequestRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OwnerRequest request)
    {
        await _context.OwnerRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }

    public async Task<OwnerRequest> GetByIdAsync(int id)
    {
        return await _context.OwnerRequests.FindAsync(id);
    }

    public async Task<List<OwnerRequest>> GetAllAsync()
    {
        return await _context.OwnerRequests
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateAsync(OwnerRequest request)
    {
        _context.OwnerRequests.Update(request);
        await _context.SaveChangesAsync();
    }
}
