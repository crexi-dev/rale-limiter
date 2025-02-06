using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using RateLimiter.Domain.Models;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
    [Test]
    public async Task RequestsTimespanPerLastRequestRuleFailTest()
    {
        var configurations = new Domain.Models.Configurations
        {
            RequestsPerTimespan = 5,
            RequestTimespan = 5,
            TimespanSinceLastCall = 5
        };

        var id = Guid.NewGuid();
        var request = new Domain.Models.RateLimiterRequest
        {
            Country = Domain.Enumerations.Contries.US,
            Id = id,
            RequestDate = DateTime.UtcNow
        };

        var updateDate = DateTime.UtcNow.AddSeconds(-3);
        var stats = new Domain.Models.RateLimiterStats
        {
            Id = id,
            LastRequestDateTime = updateDate,
            NumberOfRequestsInTimespan = 0
        };

        var result = new Domain.Models.RulesResult
        {
            updatedRateLimiterStats = stats
        };

        var rule = new Domain.Rules.RequestsTimespanPerLastRequestRule();
        var results = await rule.ExecuteRule(request, configurations, stats);

        Assert.That(results.Message == "Timespan has not expired.");
        Assert.That(results.Status == false);
        Assert.That(results.updatedRateLimiterStats.LastRequestDateTime == updateDate);
        Assert.That(results.updatedRateLimiterStats.NumberOfRequestsInTimespan == 0);
    }

    [Test]
    public async Task RequestsTimespanPerLastRequestRuleOverTimespanPassTest()
    {
        var configurations = new Domain.Models.Configurations
        {
            RequestsPerTimespan = 5,
            RequestTimespan = 5,
            TimespanSinceLastCall = 5
        };

        var id = Guid.NewGuid();
        var request = new Domain.Models.RateLimiterRequest
        {
            Country = Domain.Enumerations.Contries.US,
            Id = id,
            RequestDate = DateTime.UtcNow
        };

        var updateDate = DateTime.UtcNow.AddSeconds(-6);
        var stats = new Domain.Models.RateLimiterStats
        {
            Id = id,
            LastRequestDateTime = updateDate,
            NumberOfRequestsInTimespan = 0
        };

        var result = new Domain.Models.RulesResult
        {
            updatedRateLimiterStats = stats
        };

        var rule = new Domain.Rules.RequestsTimespanPerLastRequestRule();
        var results = await rule.ExecuteRule(request, configurations, stats);

        Assert.That(results.Message == "");
        Assert.That(results.Status == true);
        Assert.That(results.updatedRateLimiterStats.LastRequestDateTime == request.RequestDate);
        Assert.That(results.updatedRateLimiterStats.NumberOfRequestsInTimespan == 0);
    }

    [Test]
    public async Task RequestsPerTimespanRulePassTest()
    {
        var configurations = new Domain.Models.Configurations
        {
            RequestsPerTimespan = 5,
            RequestTimespan = 5,
            TimespanSinceLastCall = 5
        };

        var id = Guid.NewGuid();
        var request = new Domain.Models.RateLimiterRequest
        {
            Country = Domain.Enumerations.Contries.US,
            Id = id,
            RequestDate = DateTime.UtcNow
        };

        var updateDate = DateTime.UtcNow.AddSeconds(-3);
        var stats = new Domain.Models.RateLimiterStats
        {
            Id = id,
            LastRequestDateTime = updateDate,
            NumberOfRequestsInTimespan = 3
        };

        var result = new Domain.Models.RulesResult
        {
            updatedRateLimiterStats = stats
        };

        var rule = new Domain.Rules.RequestsPerTimespanRule();
        var results = await rule.ExecuteRule(request, configurations, stats);

        Assert.That(results.Message == "");
        Assert.That(results.Status == true);
        Assert.That(results.updatedRateLimiterStats.LastRequestDateTime == updateDate);
        Assert.That(results.updatedRateLimiterStats.NumberOfRequestsInTimespan == 4);
    }

    [Test]
    public async Task RequestsPerTimespanRuleResetTest()
    {
        var configurations = new Domain.Models.Configurations
        {
            RequestsPerTimespan = 5,
            RequestTimespan = 5,
            TimespanSinceLastCall = 5
        };

        var id = Guid.NewGuid();
        var request = new Domain.Models.RateLimiterRequest
        {
            Country = Domain.Enumerations.Contries.US,
            Id = id,
            RequestDate = DateTime.UtcNow
        };

        var updateDate = DateTime.UtcNow.AddSeconds(-6);
        var stats = new Domain.Models.RateLimiterStats
        {
            Id = id,
            LastRequestDateTime = updateDate,
            NumberOfRequestsInTimespan = 3
        };

        var result = new Domain.Models.RulesResult
        {
            updatedRateLimiterStats = stats
        };

        var rule = new Domain.Rules.RequestsPerTimespanRule();
        var results = await rule.ExecuteRule(request, configurations, stats);

        Assert.That(results.Message == "");
        Assert.That(results.Status == true);
        Assert.That(results.updatedRateLimiterStats.LastRequestDateTime == request.RequestDate);
        Assert.That(results.updatedRateLimiterStats.NumberOfRequestsInTimespan == 1);
    }

    [Test]
    public async Task RequestsPerTimespanRuleFailTest()
    {
        var configurations = new Domain.Models.Configurations
        {
            RequestsPerTimespan = 5,
            RequestTimespan = 5,
            TimespanSinceLastCall = 5
        };

        var id = Guid.NewGuid();
        var request = new Domain.Models.RateLimiterRequest
        {
            Country = Domain.Enumerations.Contries.US,
            Id = id,
            RequestDate = DateTime.UtcNow
        };

        var updateDate = DateTime.UtcNow.AddSeconds(-3);
        var stats = new Domain.Models.RateLimiterStats
        {
            Id = id,
            LastRequestDateTime = updateDate,
            NumberOfRequestsInTimespan = 6
        };

        var result = new Domain.Models.RulesResult
        {
            updatedRateLimiterStats = stats
        };

        var rule = new Domain.Rules.RequestsPerTimespanRule();
        var results = await rule.ExecuteRule(request, configurations, stats);

        Assert.That(results.Message == "To many requests for timespan.");
        Assert.That(results.Status == false);
        Assert.That(results.updatedRateLimiterStats.LastRequestDateTime == updateDate);
        Assert.That(results.updatedRateLimiterStats.NumberOfRequestsInTimespan == 6);
    }
}