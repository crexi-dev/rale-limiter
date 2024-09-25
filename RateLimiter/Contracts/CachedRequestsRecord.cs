using System.Collections.Generic;

namespace RateLimiter.Contracts;

public record CachedRequestsRecord(string RequestId, Queue<RequestDetails> Requests);