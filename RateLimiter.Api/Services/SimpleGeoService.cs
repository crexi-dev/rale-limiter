using RateLimiter.Geo;

namespace RateLimiter.Api.Services;

public class SimpleGeoService : IGeoService
{
    public async Task<string> GetLocationAsync(string clientId)
    {
        // Emulate proper implementation
        return await Task.Run(() =>
        {
            // Simplified logic for demonstration
            return clientId.StartsWith("US") ? "US" : clientId.StartsWith("EU") ? "EU" : "Other";

        });
    }
}