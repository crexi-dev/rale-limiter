using System;

namespace RateLimiter
{
    internal static class DateTimeExt
    {
        private static readonly DateTime CustomEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int GetRateValue(this DateTime dateTime, Config.TimeWindowType timeWindowType)
        {
            switch (timeWindowType)
            {
                case Config.TimeWindowType.Second:
                    return dateTime.GetRateSeconds();
                case Config.TimeWindowType.Minute:
                    return dateTime.GetRateMinutes();
                case Config.TimeWindowType.Hour:
                    return dateTime.GetRateHours();
                case Config.TimeWindowType.Day:
                    return dateTime.GetRateDays();
                case Config.TimeWindowType.Week:
                    return dateTime.GetRateWeeks();
                case Config.TimeWindowType.Month:
                    return dateTime.GetRateMonths();
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeWindowType), timeWindowType, null);
            }
        }
        public static int GetRateSeconds(this DateTime dateTime)
        {
            return (int)(dateTime.ToUniversalTime() - CustomEpoch).TotalSeconds;
        }
        public static int GetRateMinutes(this DateTime dateTime)
        {
            return (int)(dateTime.ToUniversalTime() - CustomEpoch).TotalMinutes;
        }

        public static int GetRateHours(this DateTime dateTime)
        {
            return (int)(dateTime.ToUniversalTime() - CustomEpoch).TotalHours;
        }

        public static int GetRateDays(this DateTime dateTime)
        {
            return (int)(dateTime.ToUniversalTime() - CustomEpoch).TotalDays;
        }

        public static int GetRateWeeks(this DateTime dateTime)
        {
            return (int)((dateTime.ToUniversalTime() - CustomEpoch).TotalDays / 7);
        }

        public static int GetRateMonths(this DateTime dateTime)
        {
            DateTime utcDateTime = dateTime.ToUniversalTime();
            return ((utcDateTime.Year - CustomEpoch.Year) * 12) + utcDateTime.Month - CustomEpoch.Month;
        }

        
        public static DateTime GetCustomEpoch()
        {
            return CustomEpoch;
        }
    }
}
