using RateLimiter.Repository.TrafficRepository;

namespace RateLimiter.UnitTests.TrafficRepositoryTests;

public class InMemoryTrafficRepositoryTests : ITrafficRepositoryTests
{
    #region Initialize
    private readonly InMemoryTrafficRepository _repository;

    public InMemoryTrafficRepositoryTests()
    {
        _repository = new InMemoryTrafficRepository();
    }
    #endregion

    #region Concurrency Tests
    [Fact]
    public async Task RecordTraffic_ShouldHandleMultipleConcurrentCalls()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";
        int count = 0;
        await Parallel.ForEachAsync(Enumerable.Range(1, 100), async (i, _) =>
        {
            count++;
            await _repository.RecordTraffic(new(resource, token));
        });



        var trafficEntries = await _repository.GetTraffic(token, resource);
        var sum = trafficEntries.Sum(x => x.Requests);
        Assert.Equal(100, trafficEntries.Count());
    }

    [Fact]
    public async Task GetTraffic_ShouldReturnCorrectCount_WhenCalledConcurrently()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";

        for (int i = 0; i < 100; i++)
        {
            await _repository.RecordTraffic(new(resource, token) { Time = DateTime.UtcNow.AddSeconds(-i) });
        }

        var tasks = Enumerable.Range(1, 10).Select(async _ =>
        {
            var trafficEntries = await _repository.GetTraffic(token, resource, TimeSpan.FromSeconds(50));
            return trafficEntries.Count();
        });

        var results = await Task.WhenAll(tasks);
        Assert.All(results, count => Assert.InRange(count, 50, 51));
    }

    [Fact]
    public async Task CountTraffic_ShouldReturnCorrectCount_WhenCalledConcurrently()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";

        for (int i = 0; i < 100; i++)
        {
            await _repository.RecordTraffic(new(resource, token) { Time = DateTime.UtcNow.AddSeconds(-i) });
        }

        var tasks = Enumerable.Range(1, 10).Select(async _ =>
        {
            return await _repository.CountTraffic(token, resource, TimeSpan.FromSeconds(50));
        });

        var results = await Task.WhenAll(tasks);
        Assert.All(results, count => Assert.InRange(count, 50, 51));
    }
    #endregion
}
