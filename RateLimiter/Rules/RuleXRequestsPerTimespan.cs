using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Rules
{
    public class RuleXRequestsPerTimespan : RuleTimeSpan, IRateLimitingRule
    {
        private readonly int _maxRequests;

        public RuleXRequestsPerTimespan(int maxRequests, TimeSpan timeSpan)
        {
            //Set default of maxRequests to 1 if detected to be less than 1
            _maxRequests = maxRequests < 1 ? 1 : maxRequests;

            this.timeSpan = timeSpan;
        }

        /// <summary>
        /// Check the current state based on the logged times collection for this rule.
        /// </summary>
        /// <param name="loggedTimes">The collection to check against this rule.</param>
        /// <returns>Returns RuleResult. If IsSuccess is True then the logged times collection is less than MaxRequest's value else IsSuccess is False then the logged times collection is exceeding MaxRequest's value and review the Message for more detail.</returns>
        public RuleResult Check(List<DateTime> loggedTimes)
        {
            var result = new RuleResult { RuleName = this.GetType().Name };

            //Returns success if the loggedTimes collection is empty
            if (loggedTimes.Count == 0)
                return result;

            var endTime = DateTime.UtcNow;

            var startTime = endTime - timeSpan;

            //returns all the requests where the logged time are between startTime and endTime
            var requests = loggedTimes.Where(x => x <= endTime && x >= startTime).ToList();

            if(requests.Count >= _maxRequests)
            {
                result.IsSuccess = false;

                result.Message = $"Exceeding max allowed requests of {_maxRequests} per this configured time span {timeSpan}";
            }

            return result;
        }

        /// <summary>
        /// Returns the max request value.
        /// </summary>
        /// <returns>Returns an integer value that represents the max request value.</returns>
        public int ReturnMaxRequests()
        {
            return _maxRequests;
        }
    }
}
