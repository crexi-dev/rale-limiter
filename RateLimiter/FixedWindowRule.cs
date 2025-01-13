using System;

using RateLimiter.Interfaces;

namespace RateLimiter
{
    public class FixedWindowRule
    {
        private readonly int _limit;
        private readonly TimeSpan _window;
        private readonly IUsageRepository _usageRepository;

        public FixedWindowRule(int limit, TimeSpan window, IUsageRepository usageRepository)
        {
            _limit = limit;
            _window = window;
            _usageRepository = usageRepository;
        }

        public bool IsRequestAllowed(string clientToken)
        {
            var usage = _usageRepository.GetUsageForClient(clientToken);
            var now = DateTime.UtcNow;

            if ((now - usage.WindowStart) > _window)
            {
                usage.RequestCount = 0;
                usage.WindowStart = now;
            }

            if (usage.RequestCount < _limit)
            {
                usage.RequestCount++;
                //usage.WindowStart = DateTime.UtcNow;
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
