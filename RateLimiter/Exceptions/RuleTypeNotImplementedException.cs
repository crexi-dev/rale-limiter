using System;
using RateLimiter.Constants;

namespace RateLimiter.Exceptions
{
    public class RuleTypeNotImplementedException : Exception
    {
        private const string _message = "The requested rate limit rule type has not been implemented";

        public RuleTypeNotImplementedException() : base(_message) { }

        public RuleTypeNotImplementedException(RateLimitRuleTypes rateLimitRuleType)
            : base($"{_message}: {rateLimitRuleType.ToString()}")
        { }
    }
}
