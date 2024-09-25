using RateLimiter.Contracts;
using RateLimiter.Processors;
using RateLimiter.Rules;
using Microsoft.Extensions.Logging;

namespace RateLimiter.Infrastructure;

public class EngineFactory
{
    private readonly ILogger<DefaultEngine> _logger;

    public EngineFactory(ILogger<DefaultEngine> logger)
    {
        _logger = logger;
    }
    public IContextualProcessingEngine<IRateRule, AllowRequestResult, RequestDetails> GetEngine()
    {
        return new DefaultEngine(_logger);
    }
}