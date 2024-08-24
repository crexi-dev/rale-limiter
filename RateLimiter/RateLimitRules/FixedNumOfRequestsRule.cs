using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    //This Rule allows only X number of request per timespan
    public class FixedNumOfRequestsRule : IRateLimiterRule
    {
        private int _maxRequests;
        private int _timespanSec; 
        private IDatabase _database;

        public FixedNumOfRequestsRule(int maxRequests, int timespanSec, IDatabase database)
        {
            _maxRequests = maxRequests;
            _timespanSec = timespanSec;
            _database = database;
        }

        /// <summary>
        /// Decides if request should be rate limited based on Request count and last request time
        /// </summary>
        /// <param name="token"></param>
        /// <param name="requestUTCTime"></param>
        /// <returns>bool</returns>
        public bool ShouldRateLimit(string token, DateTime requestUTCTime)
        {
            //Retrieve Request count and last request time from cache
            string currentRequestCountKey = $"{token}:RequestCount";
            string lastRequestTimeKey = $"{token}:LastRequestTime";

            int currentRequestCount = (int)(_database.StringGet(currentRequestCountKey) == RedisValue.Null ? 0 : _database.StringGet(currentRequestCountKey));
            DateTime lastRequestTime = _database.StringGet(lastRequestTimeKey) == RedisValue.Null ? DateTime.MinValue : DateTime.Parse(_database.StringGet(lastRequestTimeKey));

            // If the maximum request count is reached within the specified time window, rate limit the request
            if (currentRequestCount >= _maxRequests && (requestUTCTime - lastRequestTime).TotalSeconds < _timespanSec)
            {
                return true;
            }

            //If the current time is beyond given timespan reset the request count
            if ((requestUTCTime - lastRequestTime).TotalSeconds >= _timespanSec)
            {
                currentRequestCount = 0;
            }

            currentRequestCount++;
            //Save updated values in cache
            _database.StringSet(currentRequestCountKey, currentRequestCount);
            _database.StringSet(lastRequestTimeKey, requestUTCTime.ToString());

            return false;
        }
    }
}
