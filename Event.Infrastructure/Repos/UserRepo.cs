using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.Infrastructure.Repos
{
    public class UserRepo : IUserRepo
    {
        // learn
        private readonly ApplicationDbContext _db;
        public UserRepo(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<bool> AddUserAsync(User user)
        {
            return true;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return null;
        }

        public Task<User> GetUserByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
