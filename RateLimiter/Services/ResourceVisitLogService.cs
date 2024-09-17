using RateLimiter.Models;
using RateLimiter.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class ResourceVisitLogService : IResourceVisitLogService
    {
        private readonly IResourceVisitLogRepository _repository;
        public ResourceVisitLogService(IResourceVisitLogRepository repository) 
        {
            _repository = repository;
        }

        public async Task<Tuple<int, IEnumerable<ResourceVisitLog>>> GetResourceVisitLog(string endPoint, string sessionId)
        {
            return await _repository.GetResourceVisitLog(endPoint, sessionId);
        }
        public async Task AddResourceVisitLog(ResourceVisitLog resourceVisitLog)
        {
            await _repository.AddResourceVisitLog(resourceVisitLog);
        }
    }
}
