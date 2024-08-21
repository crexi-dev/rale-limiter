using System;
using StackExchange.Redis;

namespace RateLimiter.Services;

public class RedisConnectionManager
{
    private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
    {
        string redisConnectionString = "localhost:6379";
        return ConnectionMultiplexer.Connect(redisConnectionString);
    });

    public static ConnectionMultiplexer Connection => LazyConnection.Value;

    public static IDatabase GetDatabase() => Connection.GetDatabase();
}
