using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepository;
        public UserService(IUserRepo userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

   
        public async Task<bool> AddUserAsync(UserDto userDto)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));
            var passwordHasher = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            var user = new User(userDto.Email,passwordHasher,userDto.PhoneNumber,userDto.FirstName,userDto.LastName,1);


            var res=await  _userRepository.AddUserAsync(user);
            return res;


        }
       

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);;
            if (user == null)
                return false;
            return await _userRepository.DeleteUserAsync(user);

        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users =await _userRepository.GetAllUsersAsync();

            var userDtos = users.Select(u => new UserDto
             {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber
             }).ToList();
            return userDtos;
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user =await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return null;
            var userDto = new UserDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber

            };

            return userDto;

        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return null;
            var userDto = new UserDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
            return userDto;
        }

        public async Task<bool> UpdateUserAsync(int userId,UserDto userDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return false;
            
            if (!string.IsNullOrEmpty(userDto.FirstName))
                user.UpdateName(userDto.FirstName, user.LastName);

            if (!string.IsNullOrEmpty(userDto.LastName))
                user.UpdateName(user.FirstName, userDto.LastName);

            if(!string.IsNullOrEmpty(userDto.Email))
                user.UpdateContactInfo(userDto.Email,user.PhoneNumber);

            if (!string.IsNullOrEmpty(userDto.PhoneNumber))
                user.UpdateContactInfo(user.Email, userDto.PhoneNumber);
           
            await _userRepository.UpdateUserAsync();

            return true;

        }
        public async Task<bool> ActivateUserAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return false;
            user.Activate();
            await _userRepository.UpdateUserAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return false;
            user.Deactivate();
            await _userRepository.UpdateUserAsync();
            return true;
        }

    }
}
