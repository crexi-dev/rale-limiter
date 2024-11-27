using System.Net;

namespace RateLimiter.Components.CountryDataProvider
{
    /// <summary>
    /// This class holds a dummy sample of code that maps from an ip to a country. 
    /// Ideally this should map from ip to countries
    /// </summary>
    public class CountryDataProvider : ICountryDataProvider
    {
        public string? GetByIp(IPAddress? ip)
        {
            return "US";
        }
    }
}
