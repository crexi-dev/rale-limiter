using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace RateLimiter.Config
{
    public class Limit
    {
        public LimitType Type { get; set; }

        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan Spacing { get; set; }

        public int RequestLimit { get; set; }
               
        public TimeWindowType WindowType { get; set; }
    }
}
