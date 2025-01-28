using System;
using RateLimiter.Core.Configuration.Contracts;

namespace RateLimiter.Core.Configuration;

public class ConsoleLogger : ILogger
{
    public void LogInformation(string message) 
        => Console.WriteLine($"[INFO] {DateTime.UtcNow:O} {message}");

    public void LogWarning(string message) 
        => Console.WriteLine($"[WARN] {DateTime.UtcNow:O} {message}");

    public void LogError(string message, Exception ex = null) 
        => Console.WriteLine($"[ERROR] {DateTime.UtcNow:O} {message} {ex?.ToString()}");
}