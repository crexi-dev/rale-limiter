﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    public class ClientCallLog
    {
        public Guid ClientId { get; set; }
        public DateTime CallDateTime { get; set; }
    }
}
