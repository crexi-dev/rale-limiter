using RateLimiter.Interfaces;
using RateLimiter.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    /// <summary>
    /// Class for user repository.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IPersistentProvider _persistentProvider;

        public UserRepository(IPersistentProvider persistentProvider)
        {
            _persistentProvider = persistentProvider;
        }

        public Task AddUserAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetByUserAsync(string userId)
        {
            return await _persistentProvider.GetUserAsync(userId);
        }

        public async Task AddUserActivityAsync(string userId, string resourceId)
        {
            await _persistentProvider.AddUserAccessAsync(userId, resourceId, DateTime.UtcNow);
        }
    }
}
