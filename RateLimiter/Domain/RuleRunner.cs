using RateLimiter.Domain.Models;
using RateLimiter.Domain.Rules;
using System.Threading.Tasks;

namespace RateLimiter.Domain
{
    public class RuleRunner : Interfaces.IRuleRunner
    {
        private readonly Interfaces.IRequestReqository _requestRepository;
        public RuleRunner(Interfaces.IRequestReqository requestReqository) 
        {
            _requestRepository = requestReqository;
        }
        public async Task<Models.RulesResult> RunRules(Models.RateLimiterRequest request, Models.Configurations configurations)
        {
            RequestsPerTimespanRule requestsPerTimespanRule = new RequestsPerTimespanRule();
            RequestsTimespanPerLastRequestRule requestsTimespanPerLastRequestRule = new RequestsTimespanPerLastRequestRule();

             RateLimiterStats rateLimiterStats = _requestRepository.GetRateLimiter(request.Id);
            if (rateLimiterStats == null) {
                rateLimiterStats = new RateLimiterStats
                {
                    LastRequestDateTime = new System.DateTime()
                }; 
            }

            if (request.Country == Enumerations.Contries.US)
            {
                RulesResult requestsPerTimespanRuleResults = await requestsPerTimespanRule.ExecuteRule(request, configurations, rateLimiterStats);
                _requestRepository.SaveRateLimiter(requestsPerTimespanRuleResults.updatedRateLimiterStats);
                return requestsPerTimespanRuleResults;
            }
            else if(request.Country == Enumerations.Contries.EU)
            {
                RulesResult requestsTimespanPerLastRequestRuleResult = await requestsTimespanPerLastRequestRule.ExecuteRule(request, configurations, rateLimiterStats);
                _requestRepository.SaveRateLimiter(requestsTimespanPerLastRequestRuleResult.updatedRateLimiterStats);
                return requestsTimespanPerLastRequestRuleResult;
            }
            else
            {
                Models.RulesResult result1 = await requestsPerTimespanRule.ExecuteRule(request, configurations, rateLimiterStats);
                Models.RulesResult result2 = await requestsTimespanPerLastRequestRule.ExecuteRule(request, configurations, result1.updatedRateLimiterStats);
                _requestRepository.SaveRateLimiter(result2.updatedRateLimiterStats);
                
                return new Models.RulesResult { Message = $"{result1.Message}, {result2.Message}", Status = (result1.Status && result2.Status ) };
            }


        }
    }
}
