using System;

namespace RateLimiter.Models;

/// <summary>
/// Rate Limit Context.
/// </summary>
public class RateLimitContext 
{
    /// <summary>
    /// Client Token.
    /// </summary>
    public string ClientToken { get; }

    /// <summary>
    /// API Resource.
    /// </summary>
    public string ApiResource { get; }

    /// <summary>
    /// Region. 
    /// </summary>
    public Region Region { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitContext"/>.
    /// </summary>
    /// <param name="clientToken">The client token.</param>
    /// <param name="apiResource">The API resource.</param>
    /// <param name="region">The region.</param>
    public RateLimitContext(string clientToken, string apiResource, Region region)
    {
        ClientToken = clientToken ?? throw new ArgumentNullException(nameof(clientToken));
        ApiResource = apiResource ?? throw new ArgumentNullException(nameof(apiResource));
        Region = region;
    }
}