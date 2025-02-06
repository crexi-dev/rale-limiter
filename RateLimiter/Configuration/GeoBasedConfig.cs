namespace RateLimiter.Configuration
{
    public class GeoBasedConfig
    {
        public string Country { get; set; }
        public int Seconds { get; set; }

        public GeoBasedConfig()
        {
            Country = Configuration.Country.Default;
        }

        public GeoBasedConfig(string country, int seconds)
        {
            Country = country;
            Seconds = seconds;
        }
    }
}
