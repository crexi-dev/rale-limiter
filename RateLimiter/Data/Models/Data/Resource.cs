using RateLimiter.Data.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
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
