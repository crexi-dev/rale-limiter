using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

public class TrueRule(string ruleName) : IRateRule
{
    
    public async Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        return await Task.FromResult(new AllowRequestResult(true, "all good"));
    }

    public Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }
    
    public override string ToString()
    {
        return $"{nameof(TrueRule)}::{ruleName}";
    }
}