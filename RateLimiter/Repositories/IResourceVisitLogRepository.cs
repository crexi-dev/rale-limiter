using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    public interface IResourceVisitLogRepository
    {
        Task<Tuple<int, IEnumerable<ResourceVisitLog>>> GetResourceVisitLog(string Endpoint, string sessionId);
        Task AddResourceVisitLog(ResourceVisitLog resourceVisitLog);
    }
}
