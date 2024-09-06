using RateLimiter.Config;
using System;

namespace RateLimiter
{
    public class LimitResult
    {
        public Boolean IsSuccessful { get; set; }

        public string FailReason { get; set; }

        public Rule Rule { get; set; }

        public static readonly LimitResult Success = new LimitResult(true);

        public static readonly LimitResult Unmatched = new LimitResult(true);

        public LimitResult()
        {
            
        }
        public LimitResult(bool isSuccessful)
        {
            IsSuccessful = isSuccessful;            
        }

      
    }
}
