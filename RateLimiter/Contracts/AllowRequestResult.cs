namespace RateLimiter.Contracts;

public record AllowRequestResult(bool AllowRequest, string Reason);