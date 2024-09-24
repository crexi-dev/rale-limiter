using RateLimiter.Extensions;
using RateLimiter.Interfaces;
using RateLimiter.Models.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Persistent
{
    /// <summary>
    /// Class for one memory based implementation of IPersistentProvider.
    /// </summary>
    public class MemoryPersistentProvider : IPersistentProvider
    {
        private readonly PersistentContext _persistentContext;
        public MemoryPersistentProvider(PersistentContext persistentContext)
        {
            _persistentContext = persistentContext;
        }

        public async Task<List<Policy>?> GetUserResourcePoliciesAsync(string userId, string resourceId)
        {
            var policyIds = _persistentContext.UserResourcePolicies.FirstOrDefault(
                x => x.UserId.CaseInsensitiveEquals(userId) && x.ResourceId.CaseInsensitiveEquals(resourceId))?.Policies;
            if (policyIds?.Any() == true)
            {
                return _persistentContext.Policies.Where(x => policyIds.Contains(x.PolicyId)).ToList();
            }

            return null;
        }

        public async Task<User?> GetUserAsync(string userId)
        {
            return _persistentContext.Users.FirstOrDefault(x => x.UserId.CaseInsensitiveEquals(userId));
        }

        public async Task<Resource?> GetResourceAsync(string resourceId)
        {
            return _persistentContext.Resources.FirstOrDefault(x => x.ResourceId.CaseInsensitiveEquals(resourceId));
        }

        public async Task<Policy?> GetPolicyAsync(string policyId)
        {
            return _persistentContext.Policies.FirstOrDefault(x => x.PolicyId.CaseInsensitiveEquals(policyId));
        }

        public async Task AddUserAccessAsync(string userId, string resourceId, DateTime accessTime)
        {
            _persistentContext.UserActivities.Add(new UserActivity()
            {
                UserId = userId,
                ResourceId = resourceId,
                AccessTime = accessTime
            });
        }

        public async Task<int> GetAccessCountInSecondsAsync(string userId, string resourceId, int timeSpanSeconds)
        {
            return _persistentContext.UserActivities.Count(
                x => x.UserId.CaseInsensitiveEquals(userId) && x.ResourceId.CaseInsensitiveEquals(resourceId)
                && x.AccessTime > DateTime.UtcNow.AddSeconds(-timeSpanSeconds));
        }
    }
}
