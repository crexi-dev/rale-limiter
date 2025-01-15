using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class InMemoryRuleRepositoryTests
{
    private InMemoryRuleRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _repository = new InMemoryRuleRepository();
    }

    [Test]
    public async Task Should_Add_Rule()
    {
        var rule = new Rule("team-a:payment-api", 10, TimeSpan.FromSeconds(1));

        await _repository.AddRuleAsync(rule);

        var retrievedRule = await _repository.GetRuleAsync(rule.Scope);

        retrievedRule.Should().Be(rule);
    }
}
