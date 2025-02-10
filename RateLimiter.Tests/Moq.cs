using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using RateLimiter.Abstractions;
using RateLimiter.Exceptions;
using RateLimiter.Infrastructure;

namespace RateLimiter.Tests;

public static class Moq
{
    public static IRateLimiterRule Rule(bool isSuccess)
    {
        var mock = new Mock<IRateLimiterRule>();
        if (!isSuccess)
        {
            mock.Setup(x => x.Validate(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<DateTime>>()))
                .Throws(new RateLimitException(string.Empty, string.Empty));
        }
        return mock.Object;
    }

    public static Func<string, bool> PrefixSelector(string prefix) => x => x.StartsWith(prefix);

    public static IRequestsRepository RepositoryFromDelays(params int[] delays)
    {
        var mock = new Mock<IRequestsRepository>();
        mock.Setup(x => x.GetPreviousRequests(It.IsAny<string>()))
            .Returns(RequestsDates(delays));
        return mock.Object;
    }

    public static List<DateTime> RequestsDates(params int[] delays) => delays.Select(x => DateTime.UtcNow.AddSeconds(-x)).ToList();
}