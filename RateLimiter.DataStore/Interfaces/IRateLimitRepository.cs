using RateLimiter.DataStore.Entities;

namespace RateLimiter.DataStore.Interfaces
{
    public interface IRateLimitRepository
    {
        Task AddRequestLog(RequestLog requestLog);

        Task DeleteExpiredLogs(int pastHours);

        Task UpdateRequestLogCounts(RequestLog requestLog);

        Task<RequestLog?> GetLogByTimeStamp(
            string token,
            string resourceName,
            string timeStamp);

        Task<IList<RequestLog>> GetLogsWithinTimeSpan(
            string token,
            string resourceName,
            TimeSpan timeSpan);
    }
}
