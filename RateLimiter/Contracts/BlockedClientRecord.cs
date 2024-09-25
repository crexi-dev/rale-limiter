using System;

namespace RateLimiter.Contracts;

public record BlockedClientRecord(string RequestId, DateTime BlockExpires);