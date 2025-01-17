

using RateLimiter;
using RateLimiter.Ruls;
using RateLimiter.Ruls.Abstract;
using RateLimiter.User;

internal class Program
{
    private static void Main(string[] args)
    {
        var rules = new RateLimiterRuleDecorator[] { new MaxRequestAmountInTimeSpanRule(TimeSpan.FromSeconds(1), 5) };

        var userData = new UserData() { CountryCode = "US", Token = "tempToken", IpAddress = "192.168.18.22" };

        // Act
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        new ConcreteRateLimiter(rules).IsAllowed(userData);
        var result = new ConcreteRateLimiter(rules).IsAllowed(userData);
        Console.WriteLine("Hello, World! result = " + result);
    }
}