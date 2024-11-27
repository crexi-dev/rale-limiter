using RateLimiter.Components.CountryDataProvider;
using RateLimiter.Components.Repository;
using RateLimiter.Components.RulesService.Rules.RuleAllow1000RequestPerMinForUSMatchingControllerAndAction;
using RateLimiter.Models;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Components.RuleService.Rules.RuleAllow1RequestForMatchingConfiguration
{
    public class RuleAllow1RequestForMatchingConfiguration : IRateLimitingRule
    {
        private readonly IDataRepository _repository;
        private readonly ICountryDataProvider _countryDataProvider;

        public RuleAllow1RequestForMatchingConfiguration(
            IDataRepository repository,
            ICountryDataProvider countryDataProvider)
        {
            _repository = repository;
            _countryDataProvider = countryDataProvider;
        }

        public virtual async Task<bool> RunAsync(RateLimitingRequestData requestData, RateLimitingRuleConfiguration ruleConfig)
        {
            /*
             * This is a dummy example of a very specific rule for only one case.
             * 
             * The dummy scenario is that this rule will onlyallow 1 request from the combination of country-controller-action-parameters 
             * specified in the configuration
             */

            // controller, action, parameters MUST match otherwise the rule is ignored
            var requestDataParameters = string.Join('-', requestData.Parameters?.ToArray() ?? new string[0]);
            var configParameters = string.Join('-', ruleConfig.Parameters?.ToArray() ?? new string[0]);
            if (!string.Equals(ruleConfig.Controller, requestData.Controller, StringComparison.InvariantCultureIgnoreCase)
                || !string.Equals(ruleConfig.Action, requestData.Action, StringComparison.InvariantCultureIgnoreCase)
                || !string.Equals(requestDataParameters, configParameters, StringComparison.InvariantCultureIgnoreCase)
                )
            {
                return true;
            }


            // country MUST match otherwise the rule is ignored
            var requestCountry = _countryDataProvider.GetByIp(requestData.Ip);
            if (!string.Equals(requestCountry, ruleConfig.Country, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }



            // lets do some work 
            var key = $"{nameof(RuleAllow1RequestForMatchingConfiguration)}-{requestData.Controller}-{requestData.Action}-{requestDataParameters}".ToLowerInvariant();

            var state = await _repository.GetStateAsync<RuleAllow1RequestForMatchingConfigurationState>(key);

            if (state == null)
            {
                // first time running
                state = new RuleAllow1RequestForMatchingConfigurationState();

                await _repository.SaveStateAsync(key, state);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
