using Microsoft.Extensions.Logging;
using RateLimiter.Interface;
using RateLimiter.Model;
using System;

namespace RateLimiter
{
    public class RateLimiterService
    {
        private readonly ILogger<RateLimiterService> _logger;
        private readonly IRateLimiterRepository _rateLimiterRepository;
        private readonly IRequestLimitValidator _accessValidator;
        public RateLimiterService(ILogger<RateLimiterService> logger, IRateLimiterRepository rateLimiterRepository, IRequestLimitValidator accessValidator) 
        {
            _logger = logger;
            _rateLimiterRepository = rateLimiterRepository;
            _accessValidator = accessValidator;
        }

        public bool Validate(RequestDTO requestDTO)
        {
            try
            {
                if (requestDTO == null) 
                {
                    _logger.LogWarning("Request DTO is null");
                    return false;
                }
                var currentRequest = _rateLimiterRepository.Get(requestDTO);
                currentRequest.AccessTime.Add(requestDTO.CurrentTime);
                _rateLimiterRepository.Update(currentRequest);
                return _accessValidator.Validate((RequestStrategy)currentRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Validate request");
                throw;
            }
        }
    }
}
