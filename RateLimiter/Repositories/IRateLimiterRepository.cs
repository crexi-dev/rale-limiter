using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    public interface IRateLimiterRepository
    {
        IEnumerable<ResourceVisitLog>? GetVisitLogsForClient(string Endpoint, string sessionId);
    }
}
