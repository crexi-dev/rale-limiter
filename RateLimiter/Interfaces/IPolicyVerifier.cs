using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    /// <summary>
    /// Interface for policy verifier.
    /// </summary>
    public interface IPolicyVerifier
    {
        Task<int> GetAccessCountInSecondsAsync(string userId, string resourceId, int timeSpanSeconds);
    }
}
