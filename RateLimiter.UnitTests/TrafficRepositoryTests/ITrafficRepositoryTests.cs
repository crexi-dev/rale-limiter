using RateLimiter.Repository.TrafficRepository;

namespace RateLimiter.UnitTests.TrafficRepositoryTests;

public abstract class ITrafficRepositoryTests
{
    #region Initialize
    private readonly Mock<ITrafficRepository> _mock;
    public ITrafficRepositoryTests()
    {
        _mock = new Mock<ITrafficRepository>();
    }
    #endregion

    #region RecordTraffic Tests
    [Fact]
    public async Task RecordTraffic_ShouldRecordTrafficSuccessfully()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";
        var traffic = new Traffic(resource, token);

        _mock.Setup(repo => repo.RecordTraffic(traffic)).Returns(Task.CompletedTask);

        await _mock.Object.RecordTraffic(traffic);

        _mock.Verify(repo => repo.RecordTraffic(traffic), Times.Once);
    }
    #endregion

    #region GetLatestTraffic Tests
    [Fact]
    public async Task GetLatestTraffic_ShouldReturnLatestTrafficEntry_WhenTrafficExists()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";
        var expectedTraffic = new Traffic(resource, token) { Time = DateTime.UtcNow };

        _mock.Setup(repo => repo.GetLatestTraffic(token, resource)).ReturnsAsync(expectedTraffic);

        var result = await _mock.Object.GetLatestTraffic(token, resource);

        Assert.Equal(expectedTraffic, result);
        _mock.Verify(repo => repo.GetLatestTraffic(token, resource), Times.Once);
    }

    [Fact]
    public async Task GetLatestTraffic_ShouldReturnNull_WhenNoTrafficExistsForTokenAndResource()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";

        _mock.Setup(repo => repo.GetLatestTraffic(token, resource)).ReturnsAsync((Traffic?)null);

        var result = await _mock.Object.GetLatestTraffic(token, resource);

        Assert.Null(result);
        _mock.Verify(repo => repo.GetLatestTraffic(token, resource), Times.Once);
    }
    #endregion

    #region GetTraffic Tests
    [Fact]
    public async Task GetTraffic_ShouldReturnTrafficListWithinTimeSpan()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";
        var timeSpan = TimeSpan.FromMinutes(6);
        var expectedTrafficList = new List<Traffic>
        {
            new(resource, token) { Time = DateTime.UtcNow.AddMinutes(-5) },
            new(resource, token) { Time = DateTime.UtcNow.AddMinutes(-2) }
        };

        _mock.Setup(repo => repo.GetTraffic(token, resource, timeSpan)).ReturnsAsync(expectedTrafficList);

        var result = await _mock.Object.GetTraffic(token, resource, timeSpan);

        Assert.Equal(expectedTrafficList, result);
        _mock.Verify(repo => repo.GetTraffic(token, resource, timeSpan), Times.Once);
    }

    [Fact]
    public async Task GetTraffic_ShouldReturnEmptyList_WhenNoTrafficInTimeSpan()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";
        var timeSpan = TimeSpan.FromMinutes(5);
        var expectedTrafficList = new List<Traffic>();

        _mock.Setup(repo => repo.GetTraffic(token, resource, timeSpan)).ReturnsAsync(expectedTrafficList);

        var result = await _mock.Object.GetTraffic(token, resource, timeSpan);

        Assert.Empty(result);
        _mock.Verify(repo => repo.GetTraffic(token, resource, timeSpan), Times.Once);
    }
    #endregion

    #region CountTraffic Tests
    [Fact]
    public async Task CountTraffic_ShouldReturnCorrectCount_WhenEntriesWithinTimeSpanForSpecificResource()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";
        var timeSpan = TimeSpan.FromMinutes(6);

        _mock.Setup(repo => repo.CountTraffic(token, resource, timeSpan)).ReturnsAsync(2);

        var result = await _mock.Object.CountTraffic(token, resource, timeSpan);

        Assert.Equal(2, result);
        _mock.Verify(repo => repo.CountTraffic(token, resource, timeSpan), Times.Once);
    }

    [Fact]
    public async Task CountTraffic_ShouldReturnTotalCount_WhenNoTimeSpanProvidedForSpecificResource()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";

        _mock.Setup(repo => repo.CountTraffic(token, resource, null)).ReturnsAsync(3);

        var result = await _mock.Object.CountTraffic(token, resource);

        Assert.Equal(3, result);
        _mock.Verify(repo => repo.CountTraffic(token, resource, null), Times.Once);
    }
    #endregion

    #region ExpireTraffic Tests
    [Fact]
    public async Task ExpireTraffic_ShouldReturnExpiredCount_WhenEntriesWithinTimeSpanForSpecificResource()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";
        var timeSpan = TimeSpan.FromMinutes(6);

        _mock.Setup(repo => repo.ExpireTraffic(token, resource, timeSpan)).ReturnsAsync(2);

        var expiredCount = await _mock.Object.ExpireTraffic(token, resource, timeSpan);

        Assert.Equal(2, expiredCount);
        _mock.Verify(repo => repo.ExpireTraffic(token, resource, timeSpan), Times.Once);
    }

    [Fact]
    public async Task ExpireTraffic_ShouldExpireAllTraffic_WhenNoTimeSpanProvidedForSpecificResource()
    {
        var token = Guid.NewGuid().ToString();
        var resource = "TestResource";

        _mock.Setup(repo => repo.ExpireTraffic(token, resource, null)).ReturnsAsync(3);

        var expiredCount = await _mock.Object.ExpireTraffic(token, resource);

        Assert.Equal(3, expiredCount);
        _mock.Verify(repo => repo.ExpireTraffic(token, resource, null), Times.Once);
    }
    #endregion
}
