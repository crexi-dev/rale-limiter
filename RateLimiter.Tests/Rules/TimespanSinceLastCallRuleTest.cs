using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using RateLimiter.Rules.CountPerTimespan;
using RateLimiter.Rules.TimespanSinceLastCall;
using RateLimiter.Stores;
using RateLimiter.Tests.Common;

namespace RateLimiter.Tests.Rules;

[TestFixture]
public class TimespanSinceLastCallRuleTest
{
    private static Random Random = new Random();
    private TestTimeProvider timeProvider;
    private Mock<IRequestCountStore> store;
    private TimespanSinceLastCallRuleOptions options;
    private TimespanSinceLastCallRule sut;

    [SetUp]
    public void Init()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        this.timeProvider = new TestTimeProvider(now);

        this.store = new Mock<IRequestCountStore>();
        this.options = new TimespanSinceLastCallRuleOptions(TimeSpan.FromSeconds(Random.Next(1, 1000)));

        this.store.Setup(s => s.RequestCountSince(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Returns((string id, DateTimeOffset date) =>
        {
            if (date < now)
            {
                return Task.FromResult<long>(Random.Next(1, 100));
            }
            else
            {
                return Task.FromResult<long>(0);
            }
        });

        this.sut = new TimespanSinceLastCallRule(
            "test",
            this.timeProvider,
            this.store.Object,
            this.options);
    }

    [Test]
    public async Task AllowRequest()
    {
        DateTimeOffset now = this.timeProvider.Now();
        this.timeProvider.SetNow(now.Add(this.options.TimeSpan).AddSeconds(1));

        bool allow = await this.sut.Allow(new Client("test", "US"));

        Assert.That(allow, Is.EqualTo(true));
    }

    [Test]
    public async Task BlockRequest()
    {
        DateTimeOffset now = this.timeProvider.Now();
        this.timeProvider.SetNow(now.Add(this.options.TimeSpan).AddSeconds(-5));

        bool allow = await this.sut.Allow(new Client("test", "US"));

        Assert.That(allow, Is.EqualTo(false));
    }
}