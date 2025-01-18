namespace Crexi.RateLimiter.Models;

public class ClientRequest
{
    public required string ClientId { get; set; }
    public required string RequestId { get; set; }
    public required DateTime RequestTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Refer to ISO-3166-2 format which is 4 characters Country-State
    /// Examples for California and British Columbia: US-CA, CA-BC 
    /// Link: https://en.wikipedia.org/wiki/ISO_3166-2
    /// </summary>
    public required string RegionCountryCode { get; set; }
    
    /// <summary>
    /// Subscription level of the user or partner
    /// Allowed values: FREE,PREMIUM,PARTNER
    /// </summary>
    public required string SubscriptionLevel { get; set; }
}