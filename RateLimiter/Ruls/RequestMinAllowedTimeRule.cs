using RateLimiter.DataStore;
using RateLimiter.Ruls.Abstract;
using RateLimiter.User;
using System;

namespace RateLimiter.Ruls
{
    public class RequestMinAllowedTimeRule : RateLimiterRuleDecorator
    {
        private readonly TimeSpan _minAllowedSpan;

        public RequestMinAllowedTimeRule(TimeSpan minAllowedSpan)
        {
            _minAllowedSpan = minAllowedSpan;
        }

        public override bool IsAllowed(IUserData userData)
        {
            var result = true;
            var key = $"{userData.Token}-last-pass-time";
            var lastPass = Cashing.Get(key);

            if (lastPass != null)
            {
                var lastPassDateTime = DateTime.Parse(lastPass);
                var timeElapsed = DateTime.Now - lastPassDateTime;
                result = timeElapsed >= _minAllowedSpan;
            }
            //only update last pass time if we let rqeuest pass 
            if (result)
            {
                Cashing.Set(key, DateTime.Now.ToString());
            }
            return result && (RateLimiterRule == null || RateLimiterRule.IsAllowed(userData));
        }
    }
}
