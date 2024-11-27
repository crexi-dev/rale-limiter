using RateLimiter.Components.Repository;
using RateLimiter.Models;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Components.RuleService.Rules.RuleNRequestPerTimerange
{
    public class RuleNRequestPerTimerange : IRateLimitingRule
    {
        private readonly IDataRepository _repository;

        public RuleNRequestPerTimerange(IDataRepository repository)
        {
            _repository = repository;
        }

        public virtual async Task<bool> RunAsync(RateLimitingRequestData requestData, RateLimitingRuleConfiguration ruleConfig)
        {
            /*
             * this is an example of a reusable rule that can be applied to multiple scenarios
             */

            var parametersKey = string.Join('-', requestData.Parameters?.ToArray() ?? new string[0]);
            var key = $"{nameof(RuleNRequestPerTimerange)}-{requestData.Controller}-{requestData.Action}-{parametersKey}".ToLowerInvariant();

            var state = await _repository.GetStateAsync<RuleNRequestPerTimerangeState>(key);

            if (state == null)
            {
                // first time running
                state = new RuleNRequestPerTimerangeState()
                {
                    Timestamp = DateTime.UtcNow,
                    Counter = 1
                };

                await _repository.SaveStateAsync(key, state);

                return true;
            }

            if (IsInsideRange(state.Timestamp, ruleConfig.Timerange))
            {
                if (state.Counter + 1 <= ruleConfig.NumberOfRequests)
                {
                    // inside limits
                    state.Counter++;
                    await _repository.SaveStateAsync(key, state);
                    return true;
                }
                else
                {
                    // over quota
                    return false;
                }
            }
            else
            {
                // after time range, we start again
                state.Counter = 1;
                state.Timestamp = DateTime.UtcNow;

                await _repository.SaveStateAsync(key, state);
                return true;
            }
        }

        public virtual bool IsInsideRange(DateTime stateTimestamp, TimeSpan range)
        {
            var currentTimerange = DateTime.UtcNow - stateTimestamp;
            var result = currentTimerange <= range;

            return result;
        }
    }
}
