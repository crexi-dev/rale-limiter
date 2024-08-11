using Microsoft.Extensions.Logging;
using RateLimiter.Interface;
using RateLimiter.Model;

namespace RateLimiter.Rule.RequestPerTimeSpan
{
    public class PerTimeSpanValidator : IRateLimiterRule
    {
        public List<string> SupportedRegion => _supportedRegions.ToList();

        private int _maxRequestCount;
        private readonly TimeSpan _periodInSeconds;
        private readonly ILogger<PerTimeSpanValidator> _logger;
        private readonly IEnumerable<string> _supportedRegions;

        public PerTimeSpanValidator(int maxReqeustCount, int periodInSeconds, ILogger<PerTimeSpanValidator>logger, IEnumerable<string> supportedRegions) 
        {
            _maxRequestCount = maxReqeustCount;
            _periodInSeconds = new TimeSpan(0, 0, 0, periodInSeconds);
            _logger = logger;
            _supportedRegions = supportedRegions;
        }
        public bool VerifyAccess(Request request)
        {
            try
            {
                if (request == null)
                {
                    return false;
                }
                if (!request.AccessTime.Any())
                    return false;

                var beginningTimespan = DateTime.Now.Subtract(_periodInSeconds);
                var requestsPerTimespan = request.AccessTime.Where(x => x >= beginningTimespan);
                return _maxRequestCount >= requestsPerTimespan.Count();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Issue with Request Per Time Span Validator");
                throw;
            }
        }
    }
}