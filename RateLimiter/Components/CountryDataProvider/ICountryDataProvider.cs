using System.Net;

namespace RateLimiter.Components.CountryDataProvider
{
    public interface ICountryDataProvider
    {
        string? GetByIp(IPAddress? ip);
    }
}
