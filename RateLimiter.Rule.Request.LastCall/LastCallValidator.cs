using Microsoft.Extensions.Logging;
using RateLimiter.Interface;

namespace RateLimiter.Rule.Request.LastCall
{
    public class LastCallValidator : IRateLimiterRule
    {
        public List<string> SupportedRegion => _supportedRegions.ToList();

        private readonly ILogger _logger;
        private readonly TimeSpan _timePeriodInSeconds;
        private readonly IEnumerable<string> _supportedRegions;
        public LastCallValidator(int periodInSeconds, ILogger<LastCallValidator> logger, IEnumerable<string> supportedRegions)
        {
            _logger = logger;
            _timePeriodInSeconds = new TimeSpan(0, 0, 0, periodInSeconds);
            _supportedRegions = supportedRegions;
        }
        public bool VerifyAccess(Model.Request   request)
        {
            try
            {
                if (!request.AccessTime.Any())
                {
                    return true;
                }
                var lastAccessTime = request.AccessTime.OrderBy(x => x).Last();              
                var allowAccessTime = lastAccessTime.Add(_timePeriodInSeconds);
                return DateTime.Now >= allowAccessTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue with Last Call Validator");
                throw;
            }
        }
    }
}