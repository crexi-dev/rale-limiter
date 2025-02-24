using System;

namespace RateLimiter.Exceptions
{
    public class RuleTypeNotImplementedException : Exception
    {
        private const string _message = "The requested rule type has not been implemented";

        public RuleTypeNotImplementedException() : base(_message) { }
    }
}
