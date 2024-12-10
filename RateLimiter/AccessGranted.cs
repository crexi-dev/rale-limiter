using RateLimiter.Domain;

namespace RateLimiter;

public class AccessGranted() 
    : RequestAccessStatus(true, "Access Granted");