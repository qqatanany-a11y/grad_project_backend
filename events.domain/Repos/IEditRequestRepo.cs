using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IEditRequestRepo
    {
        Task AddAsync(EditRequest request);
        Task<EditRequest?> GetByIdAsync(int id);
        Task<List<EditRequest>> GetAllAsync();
        Task<List<EditRequest>> GetByOwnerIdAsync(int ownerId);
        Task SaveChangesAsync();
    }
}