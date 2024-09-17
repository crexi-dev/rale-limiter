using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class Resource
    {
        [Key]
        public int Id { get; set; }
        public string EndpointUrl { get; set; }  
    }
}
