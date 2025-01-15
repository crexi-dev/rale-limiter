using System.Threading.Tasks;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterServiceTests
{
    private IRuleRepository _ruleRepository;
    private RateLimiterService _rateLimiterService;

    [SetUp]
    public void SetUp()
    {
        _ruleRepository = new InMemoryRuleRepository();
        _rateLimiterService = new RateLimiterService(_ruleRepository);
    }

    [Test]
    public async Task Test_Single_Rule_Without_Context()
    {
        await _ruleRepository.AddRuleAsync(SampleRules.GlobalRule);
        var request = new RateLimitRequest(SampleRules.GlobalRule.Scope);

        var result = await _rateLimiterService.ProcessRequestAsync(request);

    }
}
