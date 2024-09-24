using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.Entities
{
    /// <summary>
    /// Class for resource entity.
    /// </summary>
    public class Resource
    {
        public string ResourceId { get; set; }
        public string Content { get; set; }
    }
}
