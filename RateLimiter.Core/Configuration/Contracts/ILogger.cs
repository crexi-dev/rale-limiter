using System;

namespace RateLimiter.Core.Configuration.Contracts;
/// <summary>
/// It can be AWS.Logger.NLog or Microsoft.Extensions.Logging
/// </summary>
public interface ILogger
{
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message, Exception ex = null!);
}