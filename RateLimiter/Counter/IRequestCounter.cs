using System;
using System.Threading.Tasks;

namespace RateLimiter.Counter
{
    public interface IRequestCounter
    {
        Task RecordRequest(string id, DateTimeOffset utcTime);
    }


}
