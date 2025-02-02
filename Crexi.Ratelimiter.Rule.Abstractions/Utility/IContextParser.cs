using Crexi.RateLimiter.Rule.Model;
using Microsoft.AspNetCore.Http;

namespace Crexi.RateLimiter.Rule.Utility;

public interface IContextParser
{
    CallData GetCallData(HttpContext context);
}