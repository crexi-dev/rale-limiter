using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

public interface IContextualRule<TResult, in TContext>
{
    Task<TResult> Evaluate(IEnumerable<TContext> context);
}