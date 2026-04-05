using events.domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace events.domain.Repos
{
    public interface IUserRepo
    {
        public Task<User?> GetUserByIdAsync(Guid userId);
        public Task<User?> GetUserByEmailAsync(string email);
        public Task<bool> AddUserAsync(User user);
        public Task UpdateUserAsync(User user);
    }
}
