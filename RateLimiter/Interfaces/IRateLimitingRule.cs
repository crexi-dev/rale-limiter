using System;
using System.Collections.Generic;

namespace RateLimiter.Interfaces
{
    public interface IRateLimitingRule
    {
        /// <summary>
        /// Determines whether a request is allowed for the given client.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <returns>True if the request is allowed; otherwise, false.</returns>
        bool IsRequestAllowed(string clientId);
    }
}
