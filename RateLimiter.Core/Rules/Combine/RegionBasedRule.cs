using System;
using RateLimiter.Core.Configuration;
using RateLimiter.Core.Configuration.Contracts;

namespace RateLimiter.Core.Rules.Combine;

public class RegionBasedRule(
    IRateLimitRule usRule,
    IRateLimitRule euRule,
    ILogger logger = null!) : IRateLimitRule
{
    private readonly IRateLimitRule _usRule = usRule ?? throw new ArgumentNullException(nameof(usRule));
    private readonly IRateLimitRule _euRule = euRule ?? throw new ArgumentNullException(nameof(euRule));

    private readonly ILogger _logger = logger ?? new ConsoleLogger();

    public bool IsAllowed(string clientToken, string resourceKey)
    {
        try
        {
            var region = GetRegionFromToken(clientToken);
            _logger.LogInformation($"Processing {region} region request for {clientToken}");

            return region switch
            {
                "US" => _usRule.IsAllowed(clientToken, resourceKey),
                "EU" => _euRule.IsAllowed(clientToken, resourceKey),
                _ => HandleUnknownRegion(clientToken)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in RegionBasedRule.IsAllowed for {clientToken}", ex);
            return false;
        }
    }

    private bool HandleUnknownRegion(string clientToken)
    {
        _logger.LogWarning($"Unknown region for token {clientToken}");
        return false;
    }

    private static string GetRegionFromToken(string token)
    {
        try
        {
            var parts = token.Split('-');
            if (parts.Length < 1 || string.IsNullOrEmpty(parts[0]))
                throw new FormatException("Invalid token format");

            return parts[0].ToUpper();
        }
        catch
        {
            throw new FormatException($"Invalid token format: {token}");
        }
    }

    public void RecordRequest(string clientToken, string resourceKey)
    {
        var region = GetRegionFromToken(clientToken);
        switch (region)
        {
            case "US":
                _usRule.RecordRequest(clientToken, resourceKey);
                break;
            case "EU":
                _euRule.RecordRequest(clientToken, resourceKey);
                break;
            default:
                throw new NotSupportedException($"Region {region} not supported.");
        }
    }
}