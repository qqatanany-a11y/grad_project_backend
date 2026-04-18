public interface IOwnerRequestRepo
{
    Task AddAsync(OwnerRequest request);
    Task<OwnerRequest> GetByIdAsync(int id);
    Task<List<OwnerRequest>> GetAllAsync();
    Task UpdateAsync(OwnerRequest request);
}