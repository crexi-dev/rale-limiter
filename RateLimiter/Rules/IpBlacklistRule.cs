using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public class IpBlacklistRule(IEnumerable<string> blocked) : IRateLimiterRule
    {
        private readonly HashSet<string> _blockedIps = blocked.ToHashSet();

        public Task<bool> EvaluateAsync(HttpContext httpContext)
        {
            string ip = httpContext.Connection.RemoteIpAddress.ToString();

            return Task.FromResult(!_blockedIps.Contains(ip));
        }
    }
}
