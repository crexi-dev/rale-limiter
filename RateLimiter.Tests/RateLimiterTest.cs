using Cache.Extensions;
using RulesService;
using RulesService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using RateLimiter.Services;
using System.Threading.Tasks;
using RateLimiter.Interfaces;
using NSubstitute;
using Cache.Providers;
using RateLimiter.Models.Requests;
using System;
using RateLimiter.Models.Enums;
using RulesService.Models.Enums;


namespace RateLimiter.Tests;
[TestFixture]
public class RateLimiterTest
{
    private readonly ICacheProvider _cacheProvider;
    private readonly IRulesService _rulesService;
    private readonly IRateLimiterService _rateLimiterService;
    private readonly ServiceProvider _serviceProvider;
    
    public RateLimiterTest()
    {
        _serviceProvider = ConfigureServices();
        var services = new ServiceCollection();
        Assert.NotNull(_serviceProvider);
        _cacheProvider = _serviceProvider.GetRequiredService<ICacheProvider>();
        Assert.NotNull(_cacheProvider);
        _rulesService = _serviceProvider.GetRequiredService<IRulesService>();
        Assert.NotNull(_rulesService);
        _rateLimiterService = new RateLimiterService(_rulesService, _cacheProvider, Substitute.For<ILogger<RateLimiterService>>() );
        Assert.NotNull(_rateLimiterService);
    }
    [Test]
	public async Task Test_GetRateLimiterRules_MaxRate_HappyPath()
	{
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier10";
        //10 requests per 1 Hour
        for (int i = 1; i <= 10; i++) 
        {
            resp = await _rateLimiterService.GetRateLimiterRules(reqUS);
            Assert.NotNull(resp);
            Assert.That(resp.IsRateExceeded, Is.False);
        }

        resp = await _rateLimiterService.GetRateLimiterRules(reqUS);
        Assert.NotNull(resp);
        Assert.That(resp.IsRateExceeded, Is.True);
	}
    [Test]
    public async Task Test_GetRateLimiterRules_VelocityRate_HappyPath()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier101";
        //10 requests per 1 Hour
        //1 per 5 secs
        resp = await _rateLimiterService.GetRateLimiterRules(reqUS);
        Assert.NotNull(resp);
        Assert.That(resp.IsRateExceeded, Is.False);

        resp = await _rateLimiterService.GetRateLimiterRules(reqUS);
        Assert.NotNull(resp);
        Assert.That(resp.IsRateExceeded, Is.True);

        await Task.Delay(TimeSpan.FromSeconds(5));

        resp = await _rateLimiterService.GetRateLimiterRules(reqUS);
        Assert.NotNull(resp);
        Assert.That(resp.IsRateExceeded, Is.False);
    }
    [Test]
    public async Task Test_GetRateLimiterRules_Error_FileError()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier10";
        reqUS.ClientApplicationEndpoint.RulesConfigFileOverride = "FileNotFound.json";

        resp = await _rateLimiterService.GetRateLimiterRules(reqUS);
        Assert.NotNull(resp);
        Assert.AreEqual(resp.ResponseCode, ResponseCodeEnum.RulesEngineError);
        Assert.NotNull(resp.RuleServiceResponseCode);
        Assert.AreEqual(resp.RuleServiceResponseCode!, RulesServiceResponseCodeEnum.SystemError);

    }

    [Test]
    public async Task Test_GetRateLimiterRules_Error_WorkflowError()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier10";
        reqUS.ClientApplicationEndpoint.RulesWorkflowOverride = "Workflow does not exists";

        resp = await _rateLimiterService.GetRateLimiterRules(reqUS);
        Assert.NotNull(resp);
        Assert.AreEqual(resp.ResponseCode, ResponseCodeEnum.RulesEngineError);
        Assert.NotNull(resp.RuleServiceResponseCode);
        Assert.AreEqual(resp.RuleServiceResponseCode!, RulesServiceResponseCodeEnum.SystemError);
    }
    [Test]
    public async Task Test_GetRateLimiterRules_Error_MaxAttemptsError()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier10";
        reqUS.ClientApplicationEndpoint.RulesConfigFileOverride = "TestRulesJson/RateLimiterRouterRulesInfiniteLoop.json";

        resp = await _rateLimiterService.GetRateLimiterRules(reqUS);
        Assert.NotNull(resp);
        Assert.AreEqual(resp.ResponseCode, ResponseCodeEnum.RulesEngineError);
        Assert.NotNull(resp.RuleServiceResponseCode);
        Assert.AreEqual(resp.RuleServiceResponseCode!, RulesServiceResponseCodeEnum.WorkflowError);
    }

    private static ServiceProvider ConfigureServices()
    {
        ServiceProvider provider = new ServiceCollection()
            .ConfigureCache()
            .ConfigureRulesService()
            .ConfigureRateLimiter()
            .BuildServiceProvider();
        
        return provider;
    }
}