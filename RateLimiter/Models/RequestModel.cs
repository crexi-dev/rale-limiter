namespace RateLimiter.Models
{
    public record RequestModel(string RequestPath, string UserId, string OrganizationId, string IpAddress, string Region);
}
