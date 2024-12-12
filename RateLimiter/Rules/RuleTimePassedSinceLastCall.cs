using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Rules
{
    public class RuleTimePassedSinceLastCall : RuleTimeSpan, IRateLimitingRule
    {
        public RuleTimePassedSinceLastCall(TimeSpan timeAllowedSinceLastCall)
        {
            timeSpan = timeAllowedSinceLastCall;
        }

        /// <summary>
        /// Check the current state based on the logged times collection for this rule.
        /// </summary>
        /// <param name="LoggedTimes">The collection to check against this rule.</param>
        /// <returns>Returns RuleResult. If IsSuccess is True then the logged times collection is less than MaxRequest's value else IsSuccess is False then the logged times collection is exceeding MaxRequest's value and review the Message for more detail.</returns>
        public RuleResult Check(List<DateTime> LoggedTimes)
        {
            var result = new RuleResult { RuleName = this.GetType().Name };

            if (LoggedTimes.Count == 0)
                return result;

            var lastestLoggedTime = LoggedTimes.Last();

            //Calculate the time difference between now and the last logged time
            var timeElapsedSinceLastRequest = DateTime.UtcNow - lastestLoggedTime;

            if (timeElapsedSinceLastRequest < timeSpan)
            {
                result.IsSuccess = false;

                result.Message = $"Last logged time was {lastestLoggedTime} and this request is not older than {timeSpan} from the last logged request.";
            }

            return result;
        }

        public int ReturnMaxRequests()
        {
            throw new NotImplementedException();
        }
    }
}
