using RateLimiter.Enums;

namespace RateLimiter.Models;

public class RateLimitResult(RateLimitResultStatuses status = RateLimitResultStatuses.Success, string message = "")
{
    public bool IsSuccessful => Status == RateLimitResultStatuses.Success;

    public string Message { get; set; } = message;

    public RateLimitResultStatuses Status { get; set; } = status;
}