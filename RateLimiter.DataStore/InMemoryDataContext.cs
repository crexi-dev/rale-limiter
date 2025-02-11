using Microsoft.EntityFrameworkCore;
using RateLimiter.DataStore.Entities;
using RateLimiter.DataStore.Interfaces;

namespace RateLimiter.DataStore
{
    /// <summary>
    /// In-Memory data context implementation
    /// </summary>
    public class InMemoryDataContext : DbContext, IDataContext
    {
        private DbSet<RequestLog> RequestLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // In-Memory database used for simplicity
            options.UseInMemoryDatabase("RateLimiterDb");
        }

        public async Task AddRequestLog(RequestLog requestLog)
        {
            RequestLogs.Add(requestLog);
            base.SaveChanges();
        }

        /// <summary>
        /// Call this to save any changes made in Entity
        /// </summary>
        public async Task UpdateRequestLogCounts(RequestLog requestLog)
        {
            requestLog.AccessCounts += 1;
            requestLog.UpdatedTime = DateTime.UtcNow;
            base.SaveChanges();
        }

        /// <summary>
        /// Call this to remove older logs
        /// </summary>
        /// <param name="pastHours"></param>
        public async Task DeleteExpiredLogs(int pastHours)
        {
            var oldLogs = RequestLogs
                .Where(x => x.CreatedTime < DateTime.Now.AddHours(pastHours));

            RequestLogs.RemoveRange(oldLogs);
            base.SaveChanges();
        }

        public async Task<RequestLog?> GetLogByTimeStamp(string token, string resourceName, string timeStamp)
        {
            return RequestLogs
                .Where(x =>
                    x.ClientToken == token &&
                    x.ResourceName == resourceName &&
                    x.TimeStampString == timeStamp)
                .FirstOrDefault();
        }

        public async Task<IList<RequestLog>> GetLogsWithinTimeSpan(string token, string resourceName, TimeSpan timeSpan)
        {
            return RequestLogs
                .Where(x =>
                    x.ClientToken == token &&
                    x.ResourceName == resourceName.Trim() &&
                    x.CreatedTime > DateTime.UtcNow.AddTicks(-timeSpan.Ticks))
                .ToList();
        }
    }
}
