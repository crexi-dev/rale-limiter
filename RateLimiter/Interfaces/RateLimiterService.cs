using RulesService.Interfaces;
using RulesService.Models;
using RulesService.Models.Requests;
using RateLimiter.Models.Requests;
using RateLimiter.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RequestTracking.Interfaces;
using RequestTracking.Models.Requests;
using static FastExpressionCompiler.ExpressionCompiler;
using System.Collections.Immutable;
using Cache.Providers;
using Cache.Models;
using RulesEngine.Models;
using Microsoft.VisualBasic;

namespace RateLimiter.Interfaces;

public class RateLimiterService : IRateLimiterService
{
    public const int ConstMaxRecursiveness = 10;
    public const string ConstDefaultRateLimiterRuleConfigFile = "RulesJson/RateLimiterRouterRules.json";
    public const string ConstDefaultRateLimiterWorkflow = "RateLimiterRouter";
    public const string ConstCachePrefixMaxRate = "maxrate";
    public const string ConstCachePrefixVelocityRate = "velocityrate";
    private readonly RateLimiterRules _defaultRules;

    private readonly ICacheProvider _cacheProvider;
    private readonly IRulesService _ruleService;
    private readonly IRequestTrackingService _requestTrackingService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<RateLimiterService> _logger;
    private readonly CacheOptions _cacheOptions;

    public RateLimiterService(IRulesService ruleService, IRequestTrackingService requestTrackingService, ICacheProvider cacheProvider, ILogger<RateLimiterService> logger)
    {
        _ruleService = ruleService;
        _requestTrackingService = requestTrackingService;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _cacheOptions = new CacheOptions() { CacheExpiryOption = CacheExpiryOptionEnum.Absolute, ExpiryTTLSeconds = 600 };

        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        var maxRateDefaultRule = new MaxRateTimeSpanRule()
        {

            Timespan = TimeSpan.FromMinutes(1),
            Rate = 1000
        };
        var velocityDefaultRule = new VelocityTimeSpanRule()
        {
            Timespan = TimeSpan.FromMilliseconds(10),
        };

        _defaultRules = new RateLimiterRules()
        {
            Name = "DefaultRules",
            Rules = new()
            {
                new RateLimiterRule()
                {
                    Name = "DefaultRule",
                    RuleType = RuleTypeEnum.MaxRateType,
                    Priority = 1000,
                    MaxRateRule = maxRateDefaultRule

                },
                 new RateLimiterRule()
                {
                    Name = "DefaultRule",
                    RuleType = RuleTypeEnum.VelocityType,
                    Priority = 2000,
                    VelocityRule = velocityDefaultRule

                }

            }
        };
    }
    public RateLimiterRules GetDefaultRules()
    {
        return _defaultRules;
    }
    public async Task<RateLimiterResponse> GetRateLimiterRules(RateLimiterRequest request)
    {
        RateLimiterResponse response = new RateLimiterResponse() { ResponseCode = Models.Enums.ResponseCodeEnum.Success, IsRateExceeded = false };
        string trace = $"{request}";
        try
        {
            var rulesConfigFile = (request.RuleConfigFile
                                  ?? (request.ClientApplicationEndpoint.RulesConfigFileOverride ?? request.ClientApplication.RulesConfigFileOverride)
                                  )
                                  ?? ConstDefaultRateLimiterRuleConfigFile;

            var rulesWorkflow = (request.RuleWorkflow
                                  ?? (request.ClientApplicationEndpoint.RulesWorkflowOverride ?? request.ClientApplication.RulesWorkflowOverride)
                                  )
                                  ?? ConstDefaultRateLimiterWorkflow;

            trace = $"{trace}. Config File = {rulesConfigFile}, Workflow = {rulesWorkflow}";

            var ruleResponse = await GetRulesAsync(rulesConfigFile, rulesWorkflow, request);
            response.RuleServiceResponseCode = ruleResponse.Response.ResponseCode;

            if (ruleResponse.Rules == null || ruleResponse.Rules.Rules == null || ruleResponse.Rules.Rules.Count == 0)
            {
                response.RuleServiceResponseMessage = ruleResponse.Response.ResponseMessage;
                response.ResponseMessage = "Didn't get rules, using default rules";
                _logger.LogWarning($"Limitation Rules are not found, using default rule. {trace}");
            }

            RateLimiterRules rules = ruleResponse.Rules ?? _defaultRules;
            var cacheKey = GetRulesCacheKey(rulesConfigFile, rulesWorkflow, request.ClientApplicationEndpoint.ClientApplicationEndpointId.ToString());
            await _cacheProvider.Set<RateLimiterRules>(cacheKey, rules, _cacheOptions);

            var result = IsRequestRateExceeded(rules, request);
            response.IsRateExceeded = result.Exceeded;
            response.RateLimiterRule = result.Rule;
            if (!response.IsRateExceeded)
            {
                AddRequestTracking(60 * 60 * 2, request);
            }

            return response;
        }
        catch (Exception ex)
        {
            response.ResponseCode = Models.Enums.ResponseCodeEnum.SystemError;
            response.ResponseMessage = $"Exception getting rate limiter rules, {trace}. Exception = {ex.Message}";
            _logger.LogError(ex, $"{response}. {trace}");
        }
        return response;
    }

    private bool IsMaxRateExceeded(MaxRateTimeSpanRule rule, RateLimiterRequest request)
    {
        if (rule == null)
        {
            return false;
        }
        string key = GetTrackingId(request);
        GetTrackedItemsResponse? resp = default;
        var dateTime = DateTime.UtcNow;
        double seconds = rule.Timespan.TotalSeconds;
        resp = _requestTrackingService.GetTrackedItemsInfo(
            new GetTrackedItemsRequest()
            {
                Key = key,
                Start = dateTime.AddSeconds(-1 * seconds),
                End = dateTime

            });

        if (resp.Count >= rule.Rate)
        {
            return true;
        }

        return false;
    }
    private bool IsVelocityRateExceeded(VelocityTimeSpanRule rule, RateLimiterRequest request)
    {
        if (rule == null)
        {
            return false;
        }
        string key = GetTrackingId(request);
        GetLastTrackedDateTimeUtcResponse? resp = default;
        var dateTime = DateTime.UtcNow;
        double seconds = rule.Timespan.TotalSeconds;
        resp = _requestTrackingService.GetLastTrackedDateTimeUtc(
            new GetLastTrackedDateTimeUtcRequest()
            {
                Key = key

            });

        if (resp.LastTrackedDateTimeUtc.AddSeconds(seconds) > dateTime)
        {
            return true;
        }

        return false;
    }

    private (bool Exceeded, RateLimiterRule? Rule) IsRequestRateExceeded(RateLimiterRules rule, RateLimiterRequest request)
    {

        string key = GetTrackingId(request);
        var dateTime = DateTime.UtcNow;

        foreach (var r in rule.Rules)
        {
            bool exceeded = false;
            switch (r.RuleType)
            {
                case RuleTypeEnum.MaxRateType:
                    {
                        exceeded = IsMaxRateExceeded(r.MaxRateRule ?? new MaxRateTimeSpanRule(), request);
                        break;
                    }
                case RuleTypeEnum.VelocityType:
                    {
                        exceeded = IsVelocityRateExceeded(r.VelocityRule ?? new VelocityTimeSpanRule(), request);
                        break;
                    }
                default:
                    break;

            }

            if (exceeded) { return (exceeded, r); }

        }

        return (false, null);
    }

    private void AddRequestTracking(double expireAfterSec, RateLimiterRequest request)
    {
        string trackingId = GetTrackingId(request);
        AddTrackedItemRequest addTrackingRequest = new AddTrackedItemRequest() { ExpireAfterSeconds = expireAfterSec, Request = request, TrackingId = trackingId };
        _requestTrackingService.AddTrackedItem(addTrackingRequest);
    }

    private string GetTrackingId(RateLimiterRequest request)
    {
        return $"request_{request.ClientApplicationEndpoint.ClientApplicationEndpointId}";
    }

    private async Task<(RateLimiterRules? Rules, GetRulesRequest Request, GetRulesResponse Response)>
        GetRulesAsync(
         string ruleFile,
         string workflow,
         RateLimiterRequest request,
         int iteration = 0)
    {
        GetRulesRequest rulesRequest = new GetRulesRequest()
        {
            RulesFile = ruleFile,
            Workflow = workflow,
            Input = new object[] { request }
        };

        RateLimiterRules? rateRules = default(RateLimiterRules?);
        string cacheKey = $"{ruleFile}_{workflow}_{request.ClientApplicationEndpoint.ClientApplicationEndpointId}";
        rateRules = await _cacheProvider.Get<RateLimiterRules>(cacheKey);
        if (rateRules != null)
        {
            return (rateRules, rulesRequest, new GetRulesResponse()
            {
                ResponseCode = RulesService.Models.Enums.RulesServiceResponseCodeEnum.Success,
                ResponseMessage = "Returning rules from cache"
            });
        }

        iteration++;

        var rulesResponse = await _ruleService.GetRulesAsync(rulesRequest);
        if (rulesResponse.ResponseCode != RulesService.Models.Enums.RulesServiceResponseCodeEnum.Success)
        {
            return (null, rulesRequest, rulesResponse);
        }

        var results = rulesResponse.RulesResults.Where(x => x.IsSuccess);

        foreach (var result in results)
        {
            if (!result.Enabled)
            { continue; }

            if (result.Properties != null)
            {
                var success = result.Properties.TryGetValue("RateRule", out var rule);
                string? obj = GetObjectValue(rule);
                rateRules = JsonSerializer.Deserialize<RateLimiterRules>(obj ?? "", _jsonSerializerOptions);
            }
        }

        if (rateRules == null)
        {
            rulesResponse.ResponseCode = RulesService.Models.Enums.RulesServiceResponseCodeEnum.WorkflowError;
            return (null, rulesRequest, rulesResponse);
        }

        rateRules.Rules = rateRules.Rules.OrderByDescending(x => x.Priority).ToList();

        var routerRules = rateRules.Rules.FirstOrDefault(x => x.RuleType == RuleTypeEnum.RouterType && x.RouterRule != null);

        if (routerRules != null)
        {
            var rules = await ProcessRouterRule(routerRules.RouterRule!, iteration, request);
            if (rules == null)
            {
                rulesResponse.ResponseCode = RulesService.Models.Enums.RulesServiceResponseCodeEnum.WorkflowError;
                rulesResponse.ResponseMessage = $"Router Rule is not configured correctly";
                return (null, rulesRequest, rulesResponse);
            }
            else
            {
                rateRules = rules;
            }
        }

        return (rateRules, rulesRequest, rulesResponse);
    }

    private string? GetObjectValue(object? obj)
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
            _logger.LogError(ex, $"{obj}");
            return null;
        }
    }

    private string GetRulesCacheKey(string ruleFile, string workflow, string requestId)
    {
        return $"{ruleFile}_{workflow}_{requestId}";
    }

    private async Task<RateLimiterRules?> ProcessRouterRule(RouterRule rule, int iteration, RateLimiterRequest request)
    {
        var nextFileRule = rule;
        if (iteration <= ConstMaxRecursiveness)
        {
            var res = await GetRulesAsync(nextFileRule.NextRuleFile, nextFileRule.NextWorkflow, request, iteration);
            return res.Rules;
        }
        return null;
    }
}
