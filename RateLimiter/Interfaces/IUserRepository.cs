using RateLimiter.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    /// <summary>
    /// Interface for user repository.
    /// </summary>
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task RemoveUserAsync(string userId);
        Task<User?> GetByUserAsync(string userId);
        Task AddUserActivityAsync(string userId, string resourceId);
    }
}
