using RateLimiter.Domain;

namespace RateLimiter;

public class AccessDenied(string message) 
    : RequestAccessStatus(false, message);