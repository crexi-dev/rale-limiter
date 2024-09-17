using Microsoft.EntityFrameworkCore;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Repositories
{
    public class ResourceVisitLogRepository : IResourceVisitLogRepository
    {
        public ApplicationDBContext _dbContext { get; set; }

        public ResourceVisitLogRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddResourceVisitLog(ResourceVisitLog resourceVisitLog)
        {
            try
            {
                await _dbContext.ResourceVisitLogs.AddAsync(resourceVisitLog);
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Tuple<int,IEnumerable<ResourceVisitLog>>> GetResourceVisitLog(string Endpoint, string sessionId)
        {
            try
            {
                var resource = await _dbContext.Resources.Where(x => x.EndpointUrl.ToLower().Equals(Endpoint.ToLower())).FirstOrDefaultAsync();
                if (resource == null)
                {
                    return Tuple.Create<int, IEnumerable<ResourceVisitLog>>(0, new List<ResourceVisitLog>());
                }

                var visitLogs = await _dbContext.ResourceVisitLogs.Where(x => x.ResourceId == resource.Id && x.SessionId.Equals(sessionId)).ToListAsync();
                return Tuple.Create<int, IEnumerable<ResourceVisitLog>>(resource.Id, visitLogs);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteOldVisitLogs(int seconds)  // scheduled to run after every Configured number of seconds
        {
            var logs = _dbContext.ResourceVisitLogs.Where(x => (DateTime.UtcNow - x.visitTime).TotalSeconds >= seconds).ToList();
            _dbContext.ResourceVisitLogs.RemoveRange(logs);
            _dbContext.SaveChanges();
            return true;
        }
    }
}
