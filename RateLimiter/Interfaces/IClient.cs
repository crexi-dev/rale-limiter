using System;
using System.Collections.Generic;
using RateLimiter.Enums;
using RateLimiter.Models;

namespace RateLimiter.Interfaces
{
    public interface IClient
    {
        Region ReturnRegion();
        List<DateTime> ReturnLoggedTimes();
        void AddRequest();
    }
}
