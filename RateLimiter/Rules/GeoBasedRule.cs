using RateLimiter.Geo;
using RateLimiter.Storages;
using System.Threading.Tasks;

namespace RateLimiter.Rules;
public class GeoBasedRule(string country, IRateLimitRule rule, IGeoService geoService) : IRateLimitRule
{
    public async Task<bool> IsRequestAllowedAsync(string clientId, string actionKey, IRateLimitStore store)
    {
        var location = await geoService.GetLocationAsync(clientId);
        if (location == country)
        {
            return await rule.IsRequestAllowedAsync(clientId, actionKey, store);
        }
        else
        {
            // Default behavior for other locations
            return true;
        }        
    }
}