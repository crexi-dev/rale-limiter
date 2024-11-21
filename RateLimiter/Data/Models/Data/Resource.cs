using System.Collections.Generic;

namespace RateLimiter.Data.Models
{
    public class Resource : BaseModel
    {
        public string Name { get; set; }
        public int StatusId { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }

        public List<LimiterRule> LimiterRules { get; set; }

    }
}
