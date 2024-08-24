using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RateLimiter
{
    //This Rule allows request only after a certain timespan has passed since the last call
    public class MinIntervalRule : IRateLimiterRule
    {
        private int _minimumInterValSec;
        private IDatabase _database;

        public MinIntervalRule(int minimumIntervalSec, IDatabase database)
        {
            _minimumInterValSec = minimumIntervalSec;
            _database = database;
        }

        /// <summary>
        ///  Decides if request should be rate limited based on the time since last request 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="requestUTCTime"></param>
        /// <returns>bool</returns>
        public bool ShouldRateLimit(string token, DateTime requestUTCTime)
        {
            //Retrieve last request time from cache
            string lastRequestTimeKey = $"{token}:LastRequestTime";
            DateTime lastRequestTime = _database.StringGet(lastRequestTimeKey) == RedisValue.Null ? DateTime.MinValue : DateTime.Parse(_database.StringGet(lastRequestTimeKey));

            // If the request is made before the cooldown period has elapsed, rate limit the request
            if ((requestUTCTime - lastRequestTime).TotalSeconds < _minimumInterValSec)
            {
                return true;
            }

            // Save updated values in cache
            _database.StringSet(lastRequestTimeKey, requestUTCTime.ToString());
            return false;
        }
    }
}
