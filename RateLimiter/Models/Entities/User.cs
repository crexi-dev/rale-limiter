using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models.Entities
{
    /// <summary>
    /// Class for user entity.
    /// </summary>
    public class User
    {
        public string UserId { get; set; }
        public string? DisplayName { get; set; }
    }
}
