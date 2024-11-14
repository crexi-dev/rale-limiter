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

        // Need to put the rules that will deterine whether to limit 
        // usage of a resource

        //public List<LimiterRule> LimiterRules { get; set; }

        public Status Status { get; set; }
    }
}
