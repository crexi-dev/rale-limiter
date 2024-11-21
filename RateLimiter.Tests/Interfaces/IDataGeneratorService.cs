using RateLimiter.Data.Models;
using System;
using System.Collections.Generic;

namespace RateLimiter.Tests.Interfaces
{
    public interface IDataGeneratorService
    {
        public Request GenerateRequest(int id, Resource resource, User user, string identifier, bool? wasHandled);
        public Resource GenerateResource(int id, string name, Status status, List<LimiterRule> limiterRules);
        public User GenerateUser(int id, string name, Guid token, string tokenSource, bool isPriorityUser);
        public LimiterRule GenerateLimiterRule(int id, string name, string? tokenSource, int? resourceStatusId, int? numPerTimespan, int? numSeconds, bool? isPriorityUser);
    }
}
