using RateLimiter.Domain.Models;
using System;

namespace RateLimiter.Domain.Interfaces
{
    public interface IRequestReqository
    {
        RateLimiterStats GetRateLimiter(Guid id);
        void SaveRateLimiter(RateLimiterStats rateLimiterStats);
    }
}
