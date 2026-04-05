using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Event.Infrastructure.Repos

{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _db;

        public UserRepo(ApplicationDbContext db)
        {
            _db = db;
        }
       
        public async Task<bool> AddUserAsync(User user)
        {
            await _db.Users.AddAsync(user);
            int result = await _db.SaveChangesAsync();
            return result > 0;
        }


        public async Task <User?> GetUserByEmailAsync(string email)
        {
            return await _db.Users
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {

            var user = await _db.Users
                .FirstOrDefaultAsync(x => x.Id.Equals(userId));
            return user;
            
        }

       


        public async Task UpdateUserAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}