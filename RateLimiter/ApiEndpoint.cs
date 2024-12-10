using System.Net.Http;

namespace RateLimiter;

public class ApiEndpoint
{
    public string Url { get; set; }
    public HttpMethod HttpMethod { get; set; }

    public ApiEndpoint(string url, HttpMethod? httpMethod = null)
    {
        Url = url;
        HttpMethod = httpMethod ?? HttpMethod.Get;
    }
}
