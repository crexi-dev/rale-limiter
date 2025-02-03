using System;
using System.Collections.Generic;
using RateLimiter.Enums;
using RateLimiter.Models;

namespace RateLimiter.Contracts;

public interface IRequestsStorage
{
    void Add(Request request);
    
    List<Request> Get(Guid id);

    public void RemoveOldRequests(Guid id, RegionType regionType, TimeSpan interval);
}