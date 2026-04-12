using events.domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace events.domain.Repos
{
    public interface IUserRepo
    {
        public Task<User?> GetUserByIdAsync(int userId);
        public Task<User?> GetUserByEmailAsync(string email);
        public Task<bool> AddUserAsync(User user);
        public Task<bool> DeleteUserAsync(User user);
        public Task<List<User>> GetAllUsersAsync();
        public Task UpdateUserAsync();
    }
}
