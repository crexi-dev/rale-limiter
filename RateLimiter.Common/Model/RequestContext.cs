namespace RateLimiter.Common.Model;

/// <summary>
/// Represents Context Info relevant to the current request scope.
/// This contains all the configuration neccessary to perform rate limiting for the request.
/// </summary>
public record RequestContext()
{
    public RateLimitingOptions[] Options { get; set; } = [];

    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Source Token of the Client making request
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Two-Letter (Alpha-2) Iso Code that represents the country where the request originates
    /// </summary>
    public string? OriginIsoCountryCode { get; set; }
}
