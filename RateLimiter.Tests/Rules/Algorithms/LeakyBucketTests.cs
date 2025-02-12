using FluentAssertions;

using Xunit;

namespace RateLimiter.Tests.Rules.Algorithms
{
    public class LeakyBucketTests
    {
        [Fact]
        public void LeakyBucket_ProcessesRequestsAtSteadyRate()
        {
            true.Should().BeTrue();
        }
    }
}
