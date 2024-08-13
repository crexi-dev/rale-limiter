namespace RateLimiter.Configuration;

public class BasePolicySettings
{
    /// <summary>
    /// Set to false if you want to apply one policy across multiple endpoints 
    /// </summary>
    public bool IsEndpointSpecific { get; set; } = true;

    /// <summary>
    /// If set to true uses ClientIdHeaderName setting to distinct clients and apply policy based on client token
    /// </summary>
    public bool IsClientSpecific { get; set; } = true;
    
    /// <summary>
    /// HTTP header name that contains client id token
    /// </summary>
    public string ClientIdHeaderName { get; set; } = "Client-Id";
}
