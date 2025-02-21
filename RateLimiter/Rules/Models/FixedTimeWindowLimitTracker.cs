namespace RateLimiter.Rules.Models
{
    public class FixedTimeWindowLimitTracker
    {
        public FixedTimeWindowLimitTracker(uint count, long initialRequestTime) 
        {
            Count = count;
            InitialRequestTime = initialRequestTime;
        }

        public uint Count { get; private set; }

        public long InitialRequestTime { get; private set; }

        public void IncrementCount()
        {
            Count++;
        }

        public void Reset(long initialRequestTime)
        {
            Count = 1;
            InitialRequestTime = initialRequestTime;
        }
    }
}
