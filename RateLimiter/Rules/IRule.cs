﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public interface IRule
    {
        bool IsAllowed(Queue<RequestModel> requestModelList);
    }
}
