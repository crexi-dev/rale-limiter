﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class ApiResponse
    {
        public string Message { get; set; }

        public bool RateLimitError { get; set; }
    }
}
