using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Rules;

public interface IOverridableRule<TResult, TContext>
{
    Func<IEnumerable<TContext>, Task<TResult>>? Override { get; set; }
}