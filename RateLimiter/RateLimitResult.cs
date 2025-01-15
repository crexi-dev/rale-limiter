using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RateLimiter;

public class RateLimitResult
{
    public IReadOnlyCollection<string> Errors { get; }
    public bool RateLimited { get; }

    public RateLimitResult(bool rateLimited = false, IList<string>? errors = null)
    {
        if (errors != null)
        {
            Errors = new ReadOnlyCollection<string>(errors);
        }
        else
        {
            Errors = new ReadOnlyCollection<string>(new List<string>());
        }

        RateLimited = rateLimited;
    }
}
