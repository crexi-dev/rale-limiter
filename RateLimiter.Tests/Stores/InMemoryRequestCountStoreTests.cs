using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using RateLimiter.Stores;
using RateLimiter.Tests.Common;

namespace RateLimiter.Tests.Stores;

[TestFixture]
public class InMemoryRequestCountStoreTests
{
    private static Random Random = new Random();
    private TestTimeProvider timeProvider;
    private IMemoryCache cache;
    private Channel<NewRequest> channel;
    private InMemoryRequestCountStore sut;

    [SetUp]
    public void Init()
    {
        this.timeProvider = new TestTimeProvider(DateTimeOffset.UtcNow);
        this.cache = this.GetInMemoryCache();
        this.channel = Channel.CreateBounded<NewRequest>(1000);

        

        this.sut = new InMemoryRequestCountStore(
            this.cache,
            this.channel.Reader);
    }

    /// <summary>
    /// No requests have been recorded.
    /// </summary>
    [Test]
    public async Task ZeroRequestsInLastTest([Values(-5, -100, -240)] int addSeconds)
    {
        DateTimeOffset now = this.timeProvider.Now();

        long requestCount = await this.sut.RequestCountSince("testId", now.AddSeconds(addSeconds));

        Assert.That(requestCount, Is.EqualTo(0));
    }

    [Test]
    public async Task RequestsInLastTest([Values(1, 2, 5, 100, 250, 1200, 3600)] int requestsToMake)
    {
        DateTimeOffset now = this.timeProvider.Now();

        for (int i = 1; i <= requestsToMake; i++)
        {
            await this.channel.Writer.WriteAsync(new NewRequest("test", this.timeProvider.Now()));
            if (Random.Next() % 2 == 0)
            {
                this.timeProvider.Advance(TimeSpan.FromSeconds(1));
            }
        }

        this.channel.Writer.Complete();
        await this.channel.Reader.Completion;

        long requestCount = await this.sut.RequestCountSince("test", now);

        Assert.That(requestCount, Is.EqualTo(requestsToMake));
    }

    private (long[] seconds, long[] minutes, long[] hours) CreateDefaultBuffers()
    {
        return (new long[60], new long[60], new long[24]);
    }

    private IMemoryCache GetInMemoryCache()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IMemoryCache>();
    }
}