using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Identifier { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public string CreatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
