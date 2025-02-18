using RateLimiter.DataStore.Entities;
using RateLimiter.DataStore.Interfaces;

namespace RateLimiter.DataStore
{
    public class RateLimitRepository : IRateLimitRepository
    {
        private IDataContext _context;

        public RateLimitRepository(IDataContext dataContext) 
        { 
            _context = dataContext;
        }

        public async Task AddRequestLog(RequestLog requestLog)
        {
            _context.RequestLogs.Add(requestLog);
            _context.SaveChanges();
        }

        /// <summary>
        /// Call this to save any changes made in Entity
        /// </summary>
        public async Task UpdateRequestLogCounts(RequestLog requestLog)
        {
            requestLog.AccessCounts += 1;
            requestLog.UpdatedTime = DateTime.UtcNow;
            _context.SaveChanges();
        }

        /// <summary>
        /// Call this to remove older logs
        /// </summary>
        /// <param name="pastHours"></param>
        public async Task DeleteExpiredLogs(int pastHours)
        {
            var oldLogs = _context.RequestLogs
                .Where(x => x.CreatedTime < DateTime.Now.AddHours(pastHours));

            _context.RequestLogs.RemoveRange(oldLogs);
            _context.SaveChanges();
        }

        public async Task<RequestLog?> GetLogByTimeStamp(string token, string resourceName, string timeStamp)
        {
            return _context.RequestLogs
                .Where(x =>
                    x.ClientToken == token &&
                    x.ResourceName == resourceName &&
                    x.TimeStampString == timeStamp)
                .FirstOrDefault();
        }

        public async Task<IList<RequestLog>> GetLogsWithinTimeSpan(string token, string resourceName, TimeSpan timeSpan)
        {
            return _context.RequestLogs
                .Where(x =>
                    x.ClientToken == token &&
                    x.ResourceName == resourceName.Trim() &&
                    x.CreatedTime > DateTime.UtcNow.AddTicks(-timeSpan.Ticks))
                .ToList();
        }
    }
}
