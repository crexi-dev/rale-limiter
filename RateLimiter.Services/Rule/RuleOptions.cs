using RateLimiter.Services.Enums;

namespace RateLimiter.Services.Rule
{
    public class RuleOptions
    {
        /// <summary>
        /// Default to 10
        /// </summary>
        public int MaxCounts { get; set; } = 10;

        /// <summary>
        /// Default to year month day hour and minutes
        /// </summary>
        public string TimeStampFormat { get; set; } = "yyyyMMddHHmm";

        /// <summary>
        /// Default to 0
        /// </summary>
        public TimeSpan TimeSpan { get; set; } = TimeSpan.FromTicks(10);


        /// <summary>
        /// Default to USA
        /// </summary>
        public RuleType RuleType { get; set; } = RuleType.USA;
    }
}
