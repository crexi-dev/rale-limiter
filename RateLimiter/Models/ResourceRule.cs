using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class ResourceRule
    {
        [Key] 
        public int Id { get; set; }
        public int ResourceId { get; set; }  // This DB Column can be part of non-clustered index for improving performance
        public int RuleId { get; set; }
        public bool Active { get; set; }
    }
}
