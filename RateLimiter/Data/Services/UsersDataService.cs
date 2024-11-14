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
        private readonly DbRepository<User> _userRepository;

        public UsersDataService(DbRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<User>> Get()
        {
            throw new NotImplementedException();
        }
        public async Task<List<User>> Get(BaseModel searchCriteria)
        {
            throw new NotImplementedException();
        }
        public async Task<User> Get(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<User> Get(string identifier)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Add(User user)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Update(int id, User user)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
