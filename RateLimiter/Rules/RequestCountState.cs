using System;
using System.Collections.Immutable;
using RateLimiter.Common;

namespace RateLimiter.Rules
{
    public record RequestCountState(
        string ClientId, 
        ImmutableArray<long> Seconds, 
        ImmutableArray<long> Minutes, 
        ImmutableArray<long> Hours)
    {
        public long RequestsSince(
            ITimeProvider timeProvider,
            DateTimeOffset date)
        {
            var now = timeProvider.Now();

            if (date > now)
            {
                return 0;
            }

            if (Seconds.Length == 0)
            {
                return 0;
            }

            // Calculate the difference in time between now and the provided date
            var totalSeconds = (int)(now - date).TotalSeconds;

            // If the difference is less than 60 seconds, use the seconds buffer
            if (totalSeconds < 60)
            {
                int totalRequests = 0;
                for (int i = 0; i < totalSeconds; i++)
                {
                    var second = (now.Second - i) % 60;
                    totalRequests += (int)Seconds[second];
                }
                return totalRequests;
            }

            // If the difference is less than 3600 seconds (1 hour), use the minutes buffer
            var totalMinutes = totalSeconds / 60;
            if (totalSeconds < 3600)
            {
                int totalRequests = 0;
                for (int i = 0; i < totalMinutes; i++)
                {
                    var minute = (now.Minute - i) % 60;
                    totalRequests += (int)Minutes[minute];
                }
                return totalRequests;
            }

            // Otherwise, use the hours buffer
            var totalHours = totalSeconds / 3600;
            int totalRequestsInHours = 0;
            for (int i = 0; i < totalHours; i++)
            {
                var hour = (now.Hour - i) % 24;
                totalRequestsInHours += (int)Hours[hour];
            }

            return totalRequestsInHours;
        }
    }
}
