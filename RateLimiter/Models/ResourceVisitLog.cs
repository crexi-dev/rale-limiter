using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class ResourceVisitLog
    {
        [Key]
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public string SessionId { get; set; }  // DB columns (ResourceId, SessionId) can be part of non-clustered Index for better performance 
        public DateTime visitTime { get;  set; }
    }
}
