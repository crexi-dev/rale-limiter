using System;
using System.Collections.Generic;

namespace RateLimiter.Infrastructure;

public interface IRequestsRepository
{
    public IReadOnlyCollection<DateTime> GetPreviousRequests(string token);
    public void AddRequest(string token, DateTime date);
}