using System;
using System.Threading.Tasks;

namespace RateLimiter.Domain;

public interface IClientDb
{
    Task<DateTime> GetLastRequestTime(string client);
    
    Task<int> GetRequestCount(string client, TimeSpan timeSpan);
}