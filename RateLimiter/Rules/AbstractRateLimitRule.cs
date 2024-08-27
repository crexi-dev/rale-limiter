
using System.Threading;

namespace RateLimiter.Rules
{
    /// <summary>
    /// Abstract implementation of the IRateLimitRule that provides the common behavior for the
    /// allowed and denied requests count. The implementation for Evaluate is provided by the derived class 
    /// </summary>
    public abstract class AbstractRateLimitRule : IRateLimitRule
    {
        protected long _allowed;
        protected long _denied;

        /// <summary>
        /// Gets the count of the requests allowed
        /// </summary>
        public long Allowed => Interlocked.Read(ref _allowed);

        /// <summary>
        /// Gets the count of the requests denied
        /// </summary>
        public long Denied => Interlocked.Read(ref _denied);

        /// <summary>
        /// Evaluation of the rule is not implemented  
        /// </summary>
        public abstract bool Evaluate(string clientKey);
    }
}

