namespace RateLimiter.Common.Model;

/// <summary>
/// Represents request(s) from a source token at a given time.
/// </summary>
/// <param name="Token"></param>
public record Traffic(string Resource, string Token)
{
    /// <summary>
    /// Identifier for the resource that is being requested. 
    /// </summary>
    public string Resource { get; set; } = Resource;

    /// <summary>
    /// The token uniquely identifies the traffic source. Traffic entries with the same token came from the same source.
    /// </summary>
    public string Token { get; init; } = Token;

    /// <summary>
    /// The time the traffic was recorded
    /// </summary>
    public DateTime Time { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The number of requests associated with this traffic. Usually 1 but could be 2 if multiple requests happen at the same exact time.
    /// </summary>
    public int Requests { get; init; } = 1;
}

public record TrafficKey(string Resource, string Token);
