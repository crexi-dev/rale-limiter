using RateLimiter.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Data.CodeValues
{
    public static class Statuses
    {
        public static List<Status> Values = new List<Status>
        {
            new Status { Id = 1, Name = "Normal", Identifier = "Normal", CreatedBy = "SeedUser", CreatedDate = DateTime.Now },
            new Status { Id = 2, Name = "Maintenance", Identifier = "Maintenance", CreatedBy = "SeedUser", CreatedDate = DateTime.Now },
            new Status { Id = 3, Name = "Offline", Identifier = "Offline", CreatedBy = "SeedUser", CreatedDate = DateTime.Now }
        };
        public static Status Normal { get { return Values.Single(x => x.Id == 1); } }
        public static Status Maintenance { get { return Values.Single(x => x.Id == 2); } }
        public static Status Offline { get { return Values.Single(x => x.Id == 3); } }
    }
}
