using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

public class BooleanRule(string ruleName) : IRateRule
{

    public Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        return Task.FromResult(new AllowRequestResult(true, "all good"));
    }


    public Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }
    
    public override string ToString()
    {
        return $"{nameof(BooleanRule)}::{ruleName}";
    }
}