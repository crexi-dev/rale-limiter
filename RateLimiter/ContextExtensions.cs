using System;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using Microsoft.AspNetCore.Http;

namespace RateLimiter;

public class ContextExtensions : IContextExtender
{
    public async Task<RequestDetails> CreateRequestDetailsFromContext(HttpRequest request)
    { 
        return await Task.Run(async () =>
        {
            try
            {
                var locationHeader = request.Headers.TryGetValue("Location", out var locationValue)
                    ? locationValue.First()
                    : string.Empty;
                var location = locationHeader != string.Empty ? locationHeader : "US";
                var tokenHeaderValue =
                    request.Headers.TryGetValue("Token", out var tokenValue)
                        ? tokenValue.First()
                        : string.Empty;
                ArgumentNullException.ThrowIfNull(tokenHeaderValue);

                return new RequestDetails(tokenHeaderValue, location, request.Path, DateTime.Now);
            }
            catch (Exception e)
            {
                throw;
            }
        });
    }
}

public interface IContextExtender
{
    Task<RequestDetails> CreateRequestDetailsFromContext(HttpRequest request);
}