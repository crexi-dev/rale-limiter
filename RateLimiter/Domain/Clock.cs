using System;
using System.Threading.Tasks;

namespace RateLimiter.Domain;

public interface IClock
{
   Task<DateTime> Now();
}

public class Clock : IClock
{
    public Task<DateTime> Now()
    {
        return Task.FromResult(DateTime.Now);
    }
}