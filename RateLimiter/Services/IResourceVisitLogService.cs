using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public interface IResourceVisitLogService
    {
        Task<Tuple<int, IEnumerable<ResourceVisitLog>>> GetResourceVisitLog(string endPoint, string sessionId);
        Task AddResourceVisitLog(ResourceVisitLog resourceVisitLog);

    }
}
