using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

public class FalseRule(string ruleName = "") : IRateRule
{
    public async Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        if (Override != null) return await Override(context);

        return new AllowRequestResult(false, "i don't know what happened");
    }

    public Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; } = null;
    
    public override string ToString()
    {
        return $"{nameof(FalseRule)}::{ruleName}";
    }
}