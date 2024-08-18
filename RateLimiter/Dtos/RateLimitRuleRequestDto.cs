namespace RateLimiter.Dtos;

public class RateLimitRuleRequestDto
{
    public int UserId { get; set; }
    public string UserLocal { get; set; } = string.Empty;
}

