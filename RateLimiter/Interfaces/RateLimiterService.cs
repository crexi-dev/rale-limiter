using Cache.Models;
using Cache.Providers;
using RulesService.Interfaces;
using RulesService.Models;
using RulesService.Models.Requests;
using Newtonsoft.Json.Converters;
using RateLimiter.Models.Requests;
using RateLimiter.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces;

public class RateLimiterService : IRateLimiterService
{
    private const int ConstMaxRecursiveness = 10;
    private readonly IRulesService _ruleService;
    private readonly ICacheProvider _cacheProvider;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    public RateLimiterService(IRulesService ruleService, ICacheProvider cacheProvider)
    {
        _ruleService = ruleService;
        _cacheProvider = cacheProvider;
        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

    }

    public async Task<RateLimiterResponse> GetRateLimiterRules(RateLimiterRequest request)
    {
        RateLimiterResponse response = new RateLimiterResponse() { ResponseCode = Models.Enums.ResponseCodeEnum.Success, IsRateExceeded = false };
        try
        {
            var rule = await GetRuleAsync("RulesJson/RateLimiterRules.json", "RateLimiter", request);
            if (rule == null)
            {
                response.ResponseMessage = "Limitation Rule is not found";
                response.IsRequestAllowed = true;
                return response;
            }

            bool rateExceeded = await IsRequestRateExceeded(rule.MaxRate, request, "maxrate");
            if (rateExceeded)
            {
                response.IsRequestAllowed = false;
                response.IsRateExceeded = true;
                return response;
            }

            rateExceeded = await IsRequestRateExceeded(rule.VelocityRate, request, "velocity");
            if (rateExceeded)
            {
                response.IsRequestAllowed = false;
                response.IsRateExceeded = true;
                return response;
            }

            await CacheRequest(rule.MaxRate, request, "maxrate");
            await CacheRequest(rule.VelocityRate, request, "velocity");
        }
        catch (Exception ex)
        {
            response.ResponseCode = Models.Enums.ResponseCodeEnum.SystemError;
        }
        return response;
    }
    private async Task<bool> IsRequestRateExceeded(RateTimeRule? rule, RateLimiterRequest request, string prefix)
    {
        if (rule == null)
        { return true; }

        var keys = await _cacheProvider.GetKeys($"{prefix}_{request.Client?.ClientId}_{request.Endpoint?.EndpointId}");
        if (keys != null && keys.Count < rule.Rate)
        {
            return false;
        }
        return true;
    }
    private async Task CacheRequest(RateTimeRule? rule, RateLimiterRequest request, string prefix)
    {
        if (rule == null)
        { return; }
        double expirySec = (int)rule.RateSpanType * rule.RateSpan;
        CacheOptions cacheOptions = new CacheOptions() { CacheExpiryOption = CacheExpiryOptionEnum.Absolute, ExpiryTTLSeconds = expirySec };
        await _cacheProvider.Set($"{prefix}_{request.Client?.ClientId}_{request.Endpoint?.EndpointId}", request, cacheOptions);
    }
    private async Task<RateLimiterRule?> GetRuleAsync(string ruleFile, string workflow, RateLimiterRequest request, int iteration = 0)
    {
        iteration++;
        GetRulesRequest rulesRequest = new GetRulesRequest()
        {
            RulesFile = ruleFile,
            Workflow = workflow,
            Input = new object[] { request }
        };

        var rulesResponse = await _ruleService.GetRulesAsync(rulesRequest);


        if (rulesResponse.ResponseCode != RulesService.Models.Enums.ResponseCodeEnum.Success)
        {
            return null;
        }
        var results = rulesResponse.RulesResults.Where(x => x.IsSuccess);
        List<RateLimiterRule> rules = new List<RateLimiterRule>();
        foreach (var result in results)
        {
            if (!result.Enabled)
            { continue; }

            if (result.Properties != null)
            {

                var success = result.Properties.TryGetValue("RateRule", out var rule);
                string? obj = GetObjectValue(rule);

                RateLimiterRule? rateRule = JsonSerializer.Deserialize<RateLimiterRule>(obj ?? "", _jsonSerializerOptions);
                if (rateRule != null)
                {
                    rules.Add(rateRule);
                }

            }
        }

        var sortedRules = rules.OrderByDescending(x => x.Priority).ToList();
        if (sortedRules[0].NextRuleFile != null && iteration <= ConstMaxRecursiveness)
        {
            var rule = await GetRuleAsync(sortedRules[0].NextRuleFile!, workflow, request, iteration);
            return rule;
        }
        else
        {
            return sortedRules[0];
        }
    }

    public static string? GetObjectValue(object? obj)
    {
        try
        {
            switch (obj)
            {
                case null:
                    return null;
                case JsonElement jsonElement:
                    {
                        var typeOfObject = jsonElement.ValueKind;
                        var rawText = jsonElement.GetRawText(); // Retrieves the raw JSON text for the element.
                        return rawText;
                    }
                default:
                    throw new ArgumentException("Expected a JsonElement object", nameof(obj));
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
