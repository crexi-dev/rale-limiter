using AutoFixture;

using Moq.AutoMock;

using RateLimiter.Abstractions;
using RateLimiter.Common;
using RateLimiter.Rules.Algorithms;

using Xunit;

namespace RateLimiter.Tests.Rules.Algorithms
{
    public class LeakyBucketTests
    {
        [Fact]
        public void LeakyBucket_ProcessesRequestsAtSteadyRate()
        {
            var mocker = new AutoMocker();
            var fixture = new Fixture();

            // arrange
            mocker.Use<IDateTimeProvider>(new DateTimeProvider());

            var rule = mocker.CreateInstance<LeakyBucket>();
            // act

            // assert
            
            //var rule = new LeakyBucket(5, TimeSpan.FromSeconds(1), clock);

            // Fill the bucket to capacity (5 requests)
            //for (int i = 0; i < 5; i++)
            //{
            //    Assert.IsTrue(rule.IsAllowed("client1")); // ✅
            //}
            //Assert.IsFalse(rule.IsAllowed("client1"));    // ❌ (Bucket full)

            //// Wait 3 seconds (3 requests leak out)
            //clock.Advance(TimeSpan.FromSeconds(3));
            //Assert.IsTrue(rule.IsAllowed("client1"));     // ✅ (Count: 5 - 3 + 1 = 3)
            //Assert.IsTrue(rule.IsAllowed("client1"));     // ✅ (Count: 4)
            //Assert.IsTrue(rule.IsAllowed("client1"));     // ✅ (Count: 5)
            //Assert.IsFalse(rule.IsAllowed("client1"));    // ❌ (Bucket full again)
        }
    }
}
