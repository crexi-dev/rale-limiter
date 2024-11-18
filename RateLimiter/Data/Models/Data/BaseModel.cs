using System;
using System.ComponentModel.DataAnnotations;

namespace RateLimiter.Data.Models
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
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
