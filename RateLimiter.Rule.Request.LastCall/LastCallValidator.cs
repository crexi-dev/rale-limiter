using Microsoft.Extensions.Logging;
using RateLimiter.Interface;

namespace RateLimiter.Rule.Request.LastCall
{
    public class LastCallValidator : IRateLimiterRule
    {
        public List<string> SupportedRegion => _supportedRegions.ToList();

        private readonly ILogger<LastCallValidator> _logger;
        private readonly TimeSpan _timePeriodInSeconds;
        private readonly IEnumerable<string> _supportedRegions;

        public LastCallValidator(int periodInSeconds, ILogger<LastCallValidator> logger, IEnumerable<string> supportedRegions)
        {
            _logger = logger;
            _timePeriodInSeconds = TimeSpan.FromSeconds(periodInSeconds);
            _supportedRegions = supportedRegions;
        }

        public bool VerifyAccess(Model.Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                if (!request.AccessTime.Any())
                {
                    return true;
                }

                var lastAccessTime = request.AccessTime.OrderBy(x => x).Last();
                var allowAccessTime = lastAccessTime.Add(_timePeriodInSeconds);
                return DateTime.UtcNow >= allowAccessTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue with Last Call Validator");
                throw;
            }
        }
    }
}