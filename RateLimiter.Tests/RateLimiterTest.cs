using Crexi.Cache.Extensions;
using Crexi.RulesService;
using Crexi.RulesService.Interfaces;
using Crexi.Utilities.Cache.Managers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using RateLimiter.Services;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Tests;
[TestFixture]
public class RateLimiterTest
{
    private readonly ICacheManager _cacheManager;
    private readonly IRulesService _rulesService;
    private readonly IRateLimiterService _rateLimiterService;
    private readonly ServiceProvider _serviceProvider;
    public RateLimiterTest()
    {
        _serviceProvider = ConfigureServices();
        Assert.NotNull(_serviceProvider);
        _cacheManager = _serviceProvider.GetRequiredService<ICacheManager>();
        _rulesService = _serviceProvider.GetRequiredService<IRulesService>();
        _rateLimiterService = _serviceProvider.GetRequiredService<IRateLimiterService>();
        Assert.NotNull(_cacheManager);
    }
    [Test]
	public async Task Example()
	{
        var req = TestData.GetUSRequest();
        var resp = await _rateLimiterService.GetRateLimiterRules(req[0]);
        var resp2 = await _rateLimiterService.GetRateLimiterRules(req[0]);

        Assert.That(true, Is.True);
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