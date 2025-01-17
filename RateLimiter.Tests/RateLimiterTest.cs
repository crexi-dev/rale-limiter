using NUnit.Framework;
using System;
using System.Threading;
using RateLimiter.DataStore;
using System.Collections.Generic;
using RateLimiter.Ruls.Abstract;
using RateLimiter.Ruls;
using RateLimiter.User;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{	
    [Test]
    public void IsAllowedWithNoRules()
    {
        Cashing.Clear();
        // Arrange
        var rules = new RateLimiterRuleDecorator[] {  };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsTrue(result);
    }
    [Test]
    public void IsAllowedWithAllRulesPositive()
    {
        Cashing.Clear();
        Dictionary<string, int> restrictionsByCountry = new()
        {
            { "US", 2 },
            { "GE", 1 }
        };
        
        // Arrange
        var rules = new RateLimiterRuleDecorator[] { 
            new IpWhiteListRule(new string[] { "192.168.18.22", "192.168.18.23" }),
            new RequestMinAllowedTimeRule(TimeSpan.FromSeconds(1)),
            new IpBlackListRule(new string[] { "192.168.18.48", "192.168.18.49" }),
            new MaxRequestAmountInTimeSpanRule(TimeSpan.FromSeconds(1),5),
            new MaxRequestAmountInTimeSpanByCountryRule(TimeSpan.FromSeconds(1), restrictionsByCountry, 2),
            };
        
        var userData = new UserData() { CountryCode = "US",Token="tempToken",IpAddress = "192.168.18.22" };

        // Act
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);      

        // Assert
        Assert.IsTrue(result);
    }
    [Test]
    public void IpWhtieListRuleNegative()
    {
        Cashing.Clear();
        // Arrange
        var rules = new RateLimiterRuleDecorator[] { new IpWhiteListRule(new string[] { "192.168.18.22", "192.168.18.23" }) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.30" };

        // Act
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);
        

        // Assert
        Assert.IsFalse(result);
    }
    [Test]
    public void IpBlackListRulePositive()
    {
        Cashing.Clear();
        // Arrange
        var rules = new RateLimiterRuleDecorator[] { new IpBlackListRule(new string[] { "192.168.18.22", "192.168.18.23" }) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.30" };

        // Act
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsTrue(result);
    }
    [Test]
    public void IpBlackListRuleNegative()
    {
        Cashing.Clear();
        // Arrange
        var rules = new RateLimiterRuleDecorator[] { new IpBlackListRule(new string[] { "192.168.18.22", "192.168.18.23" }) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsFalse(result);
    }
    [Test]
    public void IsAllowedWithRequestMinAllowedTimePositive()
    {
        Cashing.Clear();
        // Arrange
        var rules = new RateLimiterRuleDecorator[] { new RequestMinAllowedTimeRule(TimeSpan.FromSeconds(1)) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        Thread.Sleep(2000);
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsTrue(result);
    }
    [Test]
    public void IsAllowedRequestMinAllowedTimeNegative()
    {
        Cashing.Clear();
        // Arrange
        var rules = new RateLimiterRuleDecorator[] { new RequestMinAllowedTimeRule(TimeSpan.FromSeconds(1)) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsFalse(result);
    }
    [Test]
    public void IsAllowedMaxRequestAmountInTimeSpanRulePositive()
    {
        Cashing.Clear();
        // Arrange
        var rules = new RateLimiterRuleDecorator[] { new MaxRequestAmountInTimeSpanRule(TimeSpan.FromSeconds(1),5) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsTrue(result);
    }
    [Test]
    public void IsAllowedMaxRequestAmountInTimeSpanRuleWithDeleyPositive()
    {
        Cashing.Clear();
        // Arrange
        var rules = new RateLimiterRuleDecorator[] { new MaxRequestAmountInTimeSpanRule(TimeSpan.FromSeconds(1), 2) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        Thread.Sleep(2000);
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsTrue(result);
    }
    [Test]
    public void IsAllowedMaxRequestAmountInTimeSpanRuleNegative()
    {
        Cashing.Clear();
        // Arrange
        var rules = new RateLimiterRuleDecorator[] { new MaxRequestAmountInTimeSpanRule(TimeSpan.FromSeconds(1), 2) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsFalse(result);
    }
    [Test]
    public void IsAllowedMaxRequestAmountInTimeSpanByCountryPositive()
    {
        Cashing.Clear();
        // Arrange
        Dictionary<string, int> restrictionsByCountry = new()
        {
            { "US", 2 },
            { "GE", 1 }
        };
        var rules = new RateLimiterRuleDecorator[] { new MaxRequestAmountInTimeSpanByCountryRule( TimeSpan.FromSeconds(1), restrictionsByCountry, 2) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);      
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsTrue(result);
    }
    [Test]
    public void IsAllowedMaxRequestAmountInTimeSpanByCountryNegative()
    {
        Cashing.Clear();
        // Arrange
        Dictionary<string, int> restrictionsByCountry = new()
        {
            { "US", 2 },
            { "GE", 1 }
        };
        var rules = new RateLimiterRuleDecorator[] { new MaxRequestAmountInTimeSpanByCountryRule(TimeSpan.FromSeconds(1), restrictionsByCountry, 1) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsFalse(result);
    }
    [Test]
    public void IsAllowedMaxRequestAmountInTimeSpanByCountryDefoultPositive()
    {
        Cashing.Clear();
        // Arrange
        Dictionary<string, int> restrictionsByCountry = new()
        {
            { "US", 2 },
            { "GE", 1 }
        };
        var rules = new RateLimiterRuleDecorator[] { new MaxRequestAmountInTimeSpanByCountryRule(TimeSpan.FromSeconds(1), restrictionsByCountry, 2) };

        var userData = new UserData() { CountryCode = "TR", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);       
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsTrue(result);
    }
    [Test]
    public void IsAllowedMaxRequestAmountInTimeSpanByCountryDefoultNegative()
    {
        Cashing.Clear();
        // Arrange
        Dictionary<string, int> restrictionsByCountry = new()
        {
            { "US", 2 },
            { "GE", 1 }
        };
        var rules = new RateLimiterRuleDecorator[] { new MaxRequestAmountInTimeSpanByCountryRule(TimeSpan.FromSeconds(1), restrictionsByCountry, 2) };

        var userData = new UserData() { CountryCode = "TR", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);

        // Assert
        Assert.IsFalse(result);
    }


}