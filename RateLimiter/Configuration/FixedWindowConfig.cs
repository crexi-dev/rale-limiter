namespace RateLimiter.Configuration
{
    public class FixedWindowConfig
    {
        public int Limit { get; set; }
        public int Seconds { get; set; }

        public FixedWindowConfig()
        {
            
        }

        public FixedWindowConfig(int limit, int seconds)
        {
            Limit = limit;
            Seconds = seconds;
        }
    }
}
