using Event.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.Application.IServices
{
    public interface IUserService
    {
        public Task<List<UserDto>> GetAllUsersAsync();
        public Task<UserDto> GetUserByIdAsync(int userId);
        public Task<UserDto> GetUserByEmailAsync(string email);
        public Task<bool> AddUserAsync(UserDto userDto);
        public Task<bool> DeleteUserAsync(int userId);
        public Task<bool> UpdateUserAsync(int userId,UserDto userDto);
        public Task<bool> ActivateUserAsync(int userId);
        public Task<bool> DeactivateUserAsync(int userId);
    }
}
