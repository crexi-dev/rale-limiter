using RateLimiter.Data.Models;
using System;
using System.Collections.Generic;

namespace RateLimiter.Data
{
    public static class CodeValues
    {
        public static List<Status> Statuses = new List<Status> 
        {
            new Status { Id = 1, Name = "Normal", Identifier = "Normal", CreatedBy = "SeedUser", CreatedDate = DateTime.Now },
            new Status { Id = 2, Name = "Maintenance", Identifier = "Maintenance", CreatedBy = "SeedUser", CreatedDate = DateTime.Now },
            new Status { Id = 3, Name = "Offline", Identifier = "Offline", CreatedBy = "SeedUser", CreatedDate = DateTime.Now }
        };
    }
}
