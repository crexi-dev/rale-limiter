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
using RulesService.Models;
using System.Linq;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    private readonly IRequestTrackingService _requestTrackingService;
    private readonly IRulesService _rulesService;
    private readonly IRateLimiterService _rateLimiterService;
    private readonly ICacheProvider _cacheProvider;
    private readonly ServiceProvider _serviceProvider;
    private readonly RateLimiterRules _defaultRateLimiterRules;

    public RateLimiterTest()
    {
        _serviceProvider = ConfigureServices();
        var services = new ServiceCollection();
        Assert.NotNull(_serviceProvider);

        _rulesService = _serviceProvider.GetRequiredService<IRulesService>();
        Assert.NotNull(_rulesService);

        _requestTrackingService = _serviceProvider.GetRequiredService<IRequestTrackingService>();
        Assert.NotNull(_requestTrackingService);

        _cacheProvider = _serviceProvider.GetRequiredService<ICacheProvider>();
        Assert.NotNull(_cacheProvider);

        _rateLimiterService = new RateLimiterService(_rulesService, _requestTrackingService, _cacheProvider, Substitute.For<ILogger<RateLimiterService>>());
        Assert.NotNull(_rateLimiterService);

        _defaultRateLimiterRules = _rateLimiterService.GetDefaultRules();
        Assert.NotNull(_defaultRateLimiterRules);
    }

    [Test]
    public async Task GetRateLimiterRules_MaxRate_ShouldGetCorrectRateExceeded()
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
    public async Task GetRateLimiterRules_RunMultipleThreads_ShouldNotGetErrors()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;
        int numberOfThreads = 100;
        int itemsPerThread = 1000;
        reqUS.Client.Tier = "Tier10";

        await Parallel.ForEachAsync(Enumerable.Range(0, numberOfThreads), async (i, cancellationToken) =>
        {
            for (int j = 0; j < itemsPerThread; j++)
            {
                var resp = await ExecuteRequestAsync(reqUS);
                Assert.NotNull(resp);
                Assert.AreEqual(ResponseCodeEnum.Success, resp.ResponseCode);
            }
        });

        resp = await ExecuteRequestAsync(reqUS);
        Assert.NotNull(resp);
        Assert.That(resp.IsRateExceeded, Is.True, $"{resp}");
    }

    [Test]
    public async Task GetRateLimiterRules_VelocityRate_ShouldGetCorrectRateExceeded()
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
    public async Task GetRateLimiterRules_CustomTypes_ShouldSucceedGetCorrectRule()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier20";
        reqUS.Client.DefaultStateCode = "CA";
        for (int i = 1; i <= 10; i++)
        {
            resp = await ExecuteRequestAsync(reqUS);
            Assert.NotNull(resp);
            Assert.That(resp.IsRateExceeded, Is.False, $"{resp}");
        }
        resp = await ExecuteRequestAsync(reqUS);
        Assert.NotNull(resp);
        Assert.NotNull(resp.RateLimiterRule);
        Assert.AreEqual("RateLimiterUS Tier20_CA_AZ", resp.RateLimiterRule?.Name, $"{resp}");
        Assert.That(resp.IsRateExceeded, Is.True, $"{resp}");

    }

    [Test]
    public async Task GetRateLimiterRules_FileNotFound_ShouldGetDefaultRule()
    {
        var reqUS = TestData.GetUSClientRequest(Guid.NewGuid());
        RateLimiterResponse? resp = default;

        reqUS.Client.Tier = "Tier10";
        reqUS.ClientApplicationEndpoint.RulesConfigFileOverride = "FileNotFound.json";

        resp = await ExecuteRequestAsync(reqUS);

        Assert.NotNull(resp);
        Assert.AreEqual(ResponseCodeEnum.Success, resp.ResponseCode!);

        Assert.NotNull(resp.RuleServiceResponseCode);
        Assert.AreEqual(RulesServiceResponseCodeEnum.SystemError, resp.RuleServiceResponseCode!, $"{resp}");

        var rule = _defaultRateLimiterRules.Rules.Where(x => x.MaxRateRule != null).OrderBy(x => x.Priority).FirstOrDefault();

        Assert.IsNotNull(rule);
        Assert.IsNotNull(rule?.MaxRateRule);
        for (int i = 0; i < rule?.MaxRateRule?.Rate - 1; i++)
        {
            resp = await ExecuteRequestAsync(reqUS);
        }
        Assert.IsTrue(resp.IsRateExceeded, $"{resp}");
        Assert.AreEqual("DefaultRule", resp.RateLimiterRule?.Name, $"{resp}");
    }

    [Test]
    public async Task GetRateLimiterRules_WorkflowNotExist_ShouldGetSystemErrorResponse()
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
    public async Task GetRateLimiterRules_ShouldNotRecurseIndefinetely()
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