using Microsoft.Extensions.Logging;
using RateLimiter.Interface;
using RateLimiter.Model;

namespace RateLimiter
{
    public class RateLimiter
    {
        private readonly ILogger<RateLimiter> _logger;
        private readonly IRateLimiterRepository _rateLimiterRepository;
        private readonly IRateLimiterService _accessValidator;
        public RateLimiter(ILogger<RateLimiter> logger, IRateLimiterRepository rateLimiterRepository, IRateLimiterService accessValidator) 
        {
            _logger = logger;
            _rateLimiterRepository = rateLimiterRepository;
            _accessValidator = accessValidator;
        }

        public bool Validate(RequestDTO request)
        {
            var currentRequest = _rateLimiterRepository.Get(request.CallId);
            currentRequest.AccessTime.Add(request.CurrentTime);

            return _accessValidator.Validate(currentRequest);
        }
    }
}
