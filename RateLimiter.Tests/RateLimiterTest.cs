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
using RequestTracking.Interfaces;
using RequestTracking;


namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    private readonly IRequestTrackingService _requestTrackingService;
    private readonly IRulesService _rulesService;
    private readonly IRateLimiterService _rateLimiterService;
    private readonly ServiceProvider _serviceProvider;
    
    public RateLimiterTest()
    {
        _serviceProvider = ConfigureServices();
        var services = new ServiceCollection();
        Assert.NotNull(_serviceProvider);

        _rulesService = _serviceProvider.GetRequiredService<IRulesService>();
        Assert.NotNull(_rulesService);

        _requestTrackingService = _serviceProvider.GetRequiredService<IRequestTrackingService>();
        Assert.NotNull(_requestTrackingService);

        _rateLimiterService = new RateLimiterService(_rulesService, _requestTrackingService, Substitute.For<ILogger<RateLimiterService>>() );
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
            resp = await ExecuteRequestAsync(reqUS);
            Assert.NotNull(resp);
            Assert.That(resp.IsRateExceeded, Is.False, $"i={i}, {resp}");
        }

        resp = await ExecuteRequestAsync(reqUS);
        Assert.NotNull(resp);
        Assert.That(resp.IsRateExceeded, Is.True, $"{resp}");
	}

    [Test]
    public async Task Test_GetRateLimiterRules_VelocityRate_HappyPath()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier101";
        //10 requests per 1 Hour
        //1 per 5 secs
        resp = await ExecuteRequestAsync(reqUS);
        Assert.NotNull(resp);
        Assert.That(resp.IsRateExceeded, Is.False, $"{resp}");

        resp = await ExecuteRequestAsync(reqUS);
        Assert.NotNull(resp);
        Assert.That(resp.IsRateExceeded, Is.True, $"{resp}");

        await Task.Delay(TimeSpan.FromSeconds(5));

        resp = await ExecuteRequestAsync(reqUS);
        Assert.NotNull(resp);
        Assert.That(resp.IsRateExceeded, Is.False, $"{resp}");
    }

    [Test]
    public async Task Test_GetRateLimiterRules_CustomTypes_Contains()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier20";
        reqUS.Client.DefaultStateCode = "CA";
       
        resp = await ExecuteRequestAsync(reqUS);
        Assert.NotNull(resp);
        Assert.NotNull(resp.RateLimiterRule);
        Assert.AreEqual("RateLimiterUS Tier20_CA_AZ" , resp.RateLimiterRule?.Name, $"{resp}");
        Assert.That(resp.IsRateExceeded, Is.False, $"{resp}");

    }

    [Test]
    public async Task Test_GetRateLimiterRules_FileNotFound_UseDefaultRule ()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier10";
        reqUS.ClientApplicationEndpoint.RulesConfigFileOverride = "FileNotFound.json";

        resp = await ExecuteRequestAsync(reqUS);

        Assert.NotNull(resp);
        Assert.AreEqual(ResponseCodeEnum.Success, resp.ResponseCode!);

        Assert.NotNull(resp.RuleServiceResponseCode);
        Assert.AreEqual( RulesServiceResponseCodeEnum.SystemError, resp.RuleServiceResponseCode!, $"{resp}");

        Assert.AreEqual("DefaultRule", resp.RateLimiterRule?.Name, $"{resp}");
    }

    [Test]
    public async Task Test_GetRateLimiterRules_Error_WorkflowError()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier10";
        reqUS.ClientApplicationEndpoint.RulesWorkflowOverride = "Workflow does not exists";

        resp = await ExecuteRequestAsync(reqUS);

        Assert.NotNull(resp);
        Assert.AreEqual(ResponseCodeEnum.Success, resp.ResponseCode, $"{resp}");

        Assert.NotNull(resp.RuleServiceResponseCode);
        Assert.AreEqual(RulesServiceResponseCodeEnum.SystemError, resp.RuleServiceResponseCode!, $"{resp}");
    }

    [Test]
    public async Task Test_GetRateLimiterRules_MaxAttemptsExceeded()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier10";
        reqUS.ClientApplicationEndpoint.RulesConfigFileOverride = "TestRulesJson/RateLimiterRouterRulesInfiniteLoop.json";

        resp = await ExecuteRequestAsync(reqUS);

        Assert.NotNull(resp);
        Assert.AreEqual(ResponseCodeEnum.Success, resp.ResponseCode, $"{resp}");

        Assert.NotNull(resp.RuleServiceResponseCode);
        Assert.AreEqual(RulesServiceResponseCodeEnum.WorkflowError, resp.RuleServiceResponseCode!, $"{resp}");
    }

    private static ServiceProvider ConfigureServices()
    {
        ServiceProvider provider = new ServiceCollection()
            .ConfigureCache()
            .ConfigureRulesService()
            .ConfigureRequestTracking()
            .ConfigureRateLimiter()
            .BuildServiceProvider();
        
        return provider;
    }
    private async Task<RateLimiterResponse> ExecuteRequestAsync(RateLimiterRequest request)
    {
        var resp = await _rateLimiterService.GetRateLimiterRules(request);
        return resp;
    }
}