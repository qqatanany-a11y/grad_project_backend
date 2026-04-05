using events.domain.Entities;

namespace events.domain.Repos
{
    public interface IOwnerProfileRepo
    {
        Task AddAsync(OwnerProfile ownerProfile);
    }
}