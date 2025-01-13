using System;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{

    public class CooldownRule : IRateLimitStrategy
    {
        private readonly IUsageRepository _usageRepository;
        private readonly TimeSpan _cooldown;

        public CooldownRule(IUsageRepository usageRepository, TimeSpan cooldown)
        {
            _usageRepository = usageRepository;
            _cooldown = cooldown;
        }

        public bool IsRequestAllowed(string clientToken)
        {
            var usage = _usageRepository.GetUsageForClient(clientToken);
            var now = DateTime.UtcNow;

            // if no prior usage (request count = 0), or if enough time has passed since last request,
            // then allow this request
            if (usage.RequestCount == 0 || now - usage.LastRequestTime >= _cooldown)
            {
                usage.RequestCount++;
                usage.LastRequestTime = now;
                _usageRepository.UpdateUsageForClient(clientToken, usage);
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
