using NUnit.Framework;
using RequestTracking.Services;

namespace RequestTrackingTests;

[TestFixture]
public class CacheTrackingStorageProviderTests
{
    private CacheTrackingStorageProvider _provider;

    public CacheTrackingStorageProviderTests()
    {
        _provider = new CacheTrackingStorageProvider();
    }

    [Test]
    public void AddTrackedItem_ShouldAddItemToCache()
    {
        string key = Guid.NewGuid().ToString();
        string item = "testItem";
        double expireAfterSec = 5;

        _provider.AddTrackedItem(key, item, expireAfterSec);

        var count = _provider.GetTrackedItemsCount(key, DateTime.MinValue, DateTime.MaxValue);
        Assert.AreEqual(1, count);
    }

    [Test]
    public void GetTrackedItemsCount_ShouldReturnCorrectCount()
    {
        string key = Guid.NewGuid().ToString();
        double expireAfterSec = 10;
        _provider.AddTrackedItem(key, "item1", expireAfterSec);
        Thread.Sleep(10);
        _provider.AddTrackedItem(key, "item2", expireAfterSec);

        var count = _provider.GetTrackedItemsCount(key, DateTime.MinValue, DateTime.MaxValue);
        Assert.AreEqual(2, count);
    }

    [Test]
    public void GetLastTrackedDateTime_ShouldReturnCorrectDateTime()
    {
        string key = Guid.NewGuid().ToString();
        double expireAfterSec = 10;
        _provider.AddTrackedItem(key, "item1", expireAfterSec);
        Thread.Sleep(10);
        _provider.AddTrackedItem(key, "item2", expireAfterSec);

        var lastDateTime = _provider.GetLastTrackedDateTime(key);
        Assert.AreNotEqual(DateTime.MinValue, lastDateTime);
    }

    [Test]
    public void CleanupExpiredItems_ShouldRemoveExpiredItems()
    {
        string key = Guid.NewGuid().ToString();
        _provider.AddTrackedItem(key, "item1", 1);
        _provider.AddTrackedItem(key, "item2", 5);
        Thread.Sleep(2000);

        _provider.CleanupExpiredItems(null);

        var count = _provider.GetTrackedItemsCount(key, DateTime.MinValue, DateTime.MaxValue);
        Assert.AreEqual(1, count);
    }

    [Test]
    public void AddTrackedItem_ShouldSucceedAddingConcurrently()
    {
        string key = Guid.NewGuid().ToString();
        int numberOfThreads = 10;
        int itemsPerThread = 100;

        Parallel.For(0, numberOfThreads, i =>
        {
            for (int j = 0; j < itemsPerThread; j++)
            {
                _provider.AddTrackedItem(key, $"item-{i}-{j}", 10);
            }
        });

        var count = _provider.GetTrackedItemsCount(key, DateTime.MinValue, DateTime.MaxValue);
        Assert.AreEqual(numberOfThreads * itemsPerThread, count, $"Expected {numberOfThreads * itemsPerThread}, received {count}");
    }
    [Test]
    public void CleanupTimer_ShouldRemoveExpiredItems()
    {
        string key = Guid.NewGuid().ToString();

        _provider.AddTrackedItem(key, "item1", 2);
        _provider.AddTrackedItem(key, "item2", 4);
        _provider.AddTrackedItem(key, "item3", 6);

        Assert.AreEqual(3, _provider.GetTrackedItemsCount(key, DateTime.MinValue, DateTime.MaxValue));
        Thread.Sleep(3000);

        var remainingItemsCount = _provider.GetTrackedItemsCount(key, DateTime.MinValue, DateTime.MaxValue);
        Assert.AreEqual(2, remainingItemsCount);
    }
}