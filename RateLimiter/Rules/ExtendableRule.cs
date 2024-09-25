using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;

namespace RateLimiter.Rules;

public class ExtendableRule(IRateRule rule) : Decorator(rule)
{
    public override async Task<AllowRequestResult>  Evaluate(IEnumerable<RequestDetails> context)
    {
        var baseResult = await base.Evaluate(context);
        //Implement custom logic here...
        return new AllowRequestResult(false, "placeholder");
    }
}
