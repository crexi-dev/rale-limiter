namespace RateLimiter.Domain;

public struct TokenInfo
{
    public string Client { get; set; }
    public Region Region { get; set; }
}