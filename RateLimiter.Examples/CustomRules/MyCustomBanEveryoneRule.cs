namespace RateLimiter.Examples.Rules;

public class MyCustomBanEveryoneRule() : IRateLimiterRule
{
    public Task<bool> IsRequestAllowedAsync(HttpRequest request, CancellationToken ct = default)
    {
        return Task.FromResult(false);
    }
}
