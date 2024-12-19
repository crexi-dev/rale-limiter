using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    /// <summary>
    /// A rate limiting rule that applies different rules based on the client's region.
    /// </summary>
    public class RegionBasedRule : IRateLimitingRule
    {
        private readonly RequestsPerTimespanRule _usRule;
        private readonly TimespanSinceLastCallRule _euRule;
        private readonly IClientRepository _clientRepository;

        public RegionBasedRule(RequestsPerTimespanRule usRule, TimespanSinceLastCallRule euRule, IClientRepository clientRepository)
        {
            _usRule = usRule;
            _euRule = euRule;
            _clientRepository = clientRepository;
        }

        public bool IsRequestAllowed(string accessToken)
        {
            var region = _clientRepository.GetClientRegionByAccessToken(accessToken);
            return region switch
            {
                "US" => _usRule.IsRequestAllowed(accessToken),
                "EU" => _euRule.IsRequestAllowed(accessToken),
                _ => false
            };
        }
    }
}
