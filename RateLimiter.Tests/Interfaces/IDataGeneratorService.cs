using RateLimiter.Data.Models.Data;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Interfaces
{
    public interface IDataGeneratorService
    {
        public Request GenerateRequest(int id, Resource resource, User user, string identifier, bool wasHandled);
        public Resource GenerateResource(int id, string name, Status status);
        public User GenerateUser(int id, string name, Guid token, string tokenSource);
        public LimiterRule GenerateLimiterRule(int id, string name, string? tokenSource, int? resourceStatusId, int numPerTimespan, int numSeconds);
    }
}
