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
    public class RequestsAuditService : IAuditService<Request,RequestsFilter>
    {
        protected IDataService<Request> _requestDataService;

        public RequestsAuditService(IDataService<Request> requestDataService)
        {  
            _requestDataService = requestDataService; 
        }
        
        public async Task<bool> Log(Request request)
        {
            throw new NotImplementedException();
        }
        public async Task<List<Request>> GetHistory(RequestsFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}
