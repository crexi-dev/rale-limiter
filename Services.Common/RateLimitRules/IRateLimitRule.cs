namespace Services.Common.RateLimitRules;

public interface IRateLimitRule
{
    bool IsRequestAllowed(Guid token);
}