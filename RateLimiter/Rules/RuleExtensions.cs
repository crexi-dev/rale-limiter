using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

public static class Rules
{
    public static IRateRule FromTest(Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>> test)
    {
        return new BaseRule(test);
    }
}