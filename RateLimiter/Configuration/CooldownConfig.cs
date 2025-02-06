namespace RateLimiter.Configuration
{
    public class CooldownConfig
    {
        public int Seconds { get; set; }

        public CooldownConfig()
        {
            
        }

        public CooldownConfig(int seconds)
        {
            Seconds = seconds;
        }
    }
}
