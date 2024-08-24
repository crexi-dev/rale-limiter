using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace RateLimiter
{
    public class RedisHelper
    {
        private static IDatabase _redisDatabase;
        public static IDatabase Database
        {
            get 
            {
                return _redisDatabase;
            }
        }
        static RedisHelper()
        {
            string connectionString = "localhost:6379";
            var connection = ConnectionMultiplexer.Connect(connectionString);
            _redisDatabase = connection.GetDatabase();
        }

        /// <summary>
        /// Gets Redis key by given string pattern
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns>IEnumerable<RedisKey></returns>
        public static IEnumerable<RedisKey> GetKeysByPattern(IDatabase _database, string pattern)
        {
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints()[0]);
            return server.Keys(pattern: pattern);
        }

    }
}
