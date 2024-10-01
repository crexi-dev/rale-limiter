using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using RateLimiter.Rules.CountPerTimespan;
using RateLimiter.Rules.CountrySpecific;
using RateLimiter.Rules.TimespanSinceLastCall;
using RateLimiter.Stores;
using RateLimiter.Tests.Common;

namespace RateLimiter.Tests.Rules;

[TestFixture]
public class CountryRuleRuleTests
{
    private static Random Random = new Random();
    private TestTimeProvider timeProvider;
    private Mock<IRequestCountStore> store;
    private CountPerTimespanRuleOptions countPerTimestampOptions;
    private TimespanSinceLastCallRuleOptions timespanSinceLastCallRuleOptions;
    private CountrySpecificRule sut;

    [SetUp]
    public void Init()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        this.timeProvider = new TestTimeProvider(now);

        this.store = new Mock<IRequestCountStore>();
        this.countPerTimestampOptions = new CountPerTimespanRuleOptions(Random.Next(1, 1000), TimeSpan.FromSeconds(Random.Next(1, 1000)));
        this.timespanSinceLastCallRuleOptions = new TimespanSinceLastCallRuleOptions(TimeSpan.FromSeconds(Random.Next(1, 1000)));

        this.sut = new CountrySpecificRule(
            "testCountrySpecificRule",
            new Dictionary<string, IEnumerable<IRule>>()
            {
                ["US"] = new IRule[]
                {
                    new CountPerTimespanRule(
                        "testCountPerTimespan",
                        this.timeProvider,
                        this.store.Object,
                        this.countPerTimestampOptions)
                },
                ["FR"] = new IRule[]
                {
                    new TimespanSinceLastCallRule(
                        "testTimespanSinceLastCall",
                        this.timeProvider,
                        this.store.Object,
                        this.timespanSinceLastCallRuleOptions)
                }
            });
    }

    /// <summary>
    /// No requests have been recorded.
    /// </summary>
    [Test]
    public async Task AllowUSRequest()
    {
        DateTimeOffset now = this.timeProvider.Now();
        this.store.Setup(s => s.RequestCountSince(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Returns((string id, DateTimeOffset date) =>
        {
            if (date < now)
            {
                return Task.FromResult<long>(Random.Next(this.countPerTimestampOptions.MaxCount + 1, int.MaxValue));
            }
            else
            {
                return Task.FromResult<long>(Random.Next(0, this.countPerTimestampOptions.MaxCount - 1));
            }
        });

        this.timeProvider.SetNow(now.Add(this.countPerTimestampOptions.TimeSpan).AddSeconds(1));

        bool allow = await this.sut.Allow(new Client("test", "US"));

        Assert.That(allow, Is.EqualTo(true));
    }

    [Test]
    public async Task BlockUSRequest()
    {
        DateTimeOffset now = this.timeProvider.Now();
        this.store.Setup(s => s.RequestCountSince(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Returns((string id, DateTimeOffset date) =>
        {
            if (date < now)
            {
                return Task.FromResult<long>(Random.Next(this.countPerTimestampOptions.MaxCount + 1, int.MaxValue));
            }
            else
            {
                return Task.FromResult<long>(Random.Next(0, this.countPerTimestampOptions.MaxCount - 1));
            }
        });

        this.timeProvider.SetNow(now.Add(this.countPerTimestampOptions.TimeSpan).AddSeconds(-5));

        bool allow = await this.sut.Allow(new Client("test", "US"));

        Assert.That(allow, Is.EqualTo(false));
    }

    [Test]
    public async Task AllowFRRequest()
    {
        DateTimeOffset now = this.timeProvider.Now();
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

        this.timeProvider.SetNow(now.Add(this.timespanSinceLastCallRuleOptions.TimeSpan).AddSeconds(1));

        bool allow = await this.sut.Allow(new Client("test", "FR"));

        Assert.That(allow, Is.EqualTo(true));
    }

    [Test]
    public async Task BlockFRRequest()
    {
        DateTimeOffset now = this.timeProvider.Now();
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

        this.timeProvider.SetNow(now.Add(this.timespanSinceLastCallRuleOptions.TimeSpan).AddSeconds(-5));

        bool allow = await this.sut.Allow(new Client("test", "FR"));

        Assert.That(allow, Is.EqualTo(false));
    }
}