namespace RateLimiter.Examples.Rules;

public class MyCustomRequestSizeRule(TimeSpan window, int maxBytes, IRateLimiterStorage storage, string groupId) : IRateLimiterRule
{
    private const string KeyPreffix = "mcrs_";

    public async Task<bool> IsRequestAllowedAsync(HttpRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var requestSize = request.ContentLength ?? 0;
        if (requestSize == 0)
        {
            return true;
        }

        if (!await storage.TryGetAsync<(int bytesReceived, DateTime windowStart)>(GetKey(), out var value, ct))
        {
            if (requestSize <= maxBytes)
            {
                await storage.SetAsync(GetKey(), (requestSize, now), window, ct);
                return true;
            }
        }

        var totalBytes = value.bytesReceived + requestSize;
        if (totalBytes <= maxBytes)
        {
            await storage.SetAsync(GetKey(), (totalBytes, now), window, ct);
            return true;
        }

        return false;
    }

    private string GetKey() => $"{KeyPreffix}{groupId}";
}
