using RateLimiter.Data.Interfaces;
using RateLimiter.Data.Models.Filter;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class LimiterService : ILimiterService
    {
        private readonly IAuditService<Request,RequestsFilter> _requestAuditService;

        public LimiterService(IAuditService<Request, RequestsFilter> requestAuditService)
        {
            _requestAuditService = requestAuditService;
        }

        public async Task<bool> LimitAccess(int ResourceId, int UserId)
        {
            throw new NotImplementedException();
        }
    }
}
