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

        public async Task<bool> DeleteUserAsync(User user)
        {
            _db.Users.Remove(user);
            int result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _db.Users.Include(u => u.Role)
                .ToListAsync();
        }


        public async Task <User?> GetUserByEmailAsync(string email)
        {
            return await _db.Users.
                Include(u => u.Role)
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {

            var user = await _db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(x => x.Id==userId);
            return user;
      
        }

        public async Task UpdateUserAsync()
        {
            
            await _db.SaveChangesAsync();
        }
    }
}