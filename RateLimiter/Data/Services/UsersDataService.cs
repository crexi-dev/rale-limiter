using M42.Data.Repositories;
using RateLimiter.Data.Interfaces;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class UsersDataService : IDataService<User>
    {
        private readonly DbRepository<User> _usersRepository;

        public UsersDataService(DbRepository<User> usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<List<User>> GetAllAsync()
        {
            string[] includes = new string[] { "" };

            var users = await _usersRepository.GetAllAsync(includes);

            return users;
        }
        public async Task<User> SingleAsync(int id)
        {
            string[] includes = new string[] { "" };

            var user = await _usersRepository.SingleAsync(id, includes);

            return user;
        }
        public async Task<User> SingleAsync(string identifier)
        {
            string[] includes = new string[] { "" };

            var user = await _usersRepository.SingleAsync(identifier, includes);

            return user;
        }
        public async Task<bool> AddAsync(User user)
        {
            var newUser = await _usersRepository.AddAsync(user);

            return true;
        }
        public async Task<bool> UpdateAsync(int id, User user)
        {
            string[] includes = new string[] { "" };

            var existingUser = await _usersRepository.SingleAsync(id, includes);

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Token = user.Token;
            existingUser.UpdatedBy = user.UpdatedBy;
            existingUser.UpdatedDate = DateTime.Now;

            await _usersRepository.UpdateAsync(existingUser);

            return true;
        }
        public async Task<bool> RemoveAsync(int id)
        {
            var newUser = await _usersRepository.RemoveAsync(id);

            return true;
        }
    }
}
