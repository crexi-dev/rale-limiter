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

namespace RateLimiter.Interfaces;

public class RateLimiterService : IRateLimiterService
{
    public const int ConstMaxRecursiveness = 10;
    public const string ConstDefaultRateLimiterRuleConfigFile = "RulesJson/RateLimiterRouterRules.json";
    public const string ConstDefaultRateLimiterWorkflow = "RateLimiterRouter";
    public const string ConstCachePrefixMaxRate = "maxrate";
    public const string ConstCachePrefixVelocityRate = "velocityrate";
    public readonly RateLimiterRule _defaultRule;

    private readonly IRulesService _ruleService;
    private readonly IRequestTrackingService _requestTrackingService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<RateLimiterService> _logger;

    public RateLimiterService(IRulesService ruleService, IRequestTrackingService requestTrackingService, ILogger<RateLimiterService> logger)
    {
        _ruleService = ruleService;
        _requestTrackingService = requestTrackingService;
        _logger = logger;
        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        _defaultRule = new RateLimiterRule()
        {
            Name = "DefaultRule",
            MaxRate = new RateTimeRule()
            {
                //1000 per 1 Hour
                RateSpanType = Models.Enums.RateSpanTypeEnum.Hour,
                RateSpan = 1,
                Rate = 1000
            },
            VelocityRate = new RateTimeRule()
            {
                //1 per 1 Hour
                RateSpanType = Models.Enums.RateSpanTypeEnum.Second,
                RateSpan = 1,
                Rate = 1

            }
        };
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

            var ruleResponse = await GetRuleAsync(rulesConfigFile, rulesWorkflow, request);
            response.RuleServiceResponseCode = ruleResponse.Response.ResponseCode;
          
            if (ruleResponse.Rule == null)
            {
                _logger.LogWarning($"Limitation Rule is not found, using default rule. {trace}");
            }

            RateLimiterRule rule = ruleResponse.Rule??_defaultRule;

            response.RateLimiterRule = rule;
            response.IsRateExceeded = IsRequestRateExceeded(rule, request);
            AddRequestTracking(rule.MaxRate, request);
            return response;

            //await SetRequestCache(ruleResponse.Rule.MaxRate, request, ConstCachePrefixMaxRate);
            //await SetRequestCache(ruleResponse.Rule.VelocityRate, request, ConstCachePrefixVelocityRate);
        }
        catch (Exception ex)
        {
            response.ResponseCode = Models.Enums.ResponseCodeEnum.SystemError;
            response.ResponseMessage = $"Exception getting rate limiter rules, {trace}. Exception = {ex.Message}";
            _logger.LogError(ex, $"{response}. {trace}");
        }
        return response;
    }

    private bool IsRequestRateExceeded(RateLimiterRule? rule, RateLimiterRequest request)
    {
        if(rule == null)
        {
            return false;
        }
        string key = GetTrackingId(request);
        GetTrackedItemsResponse? resp = default;
        var dateTime = DateTime.UtcNow;

        if (rule.MaxRate != null)
        {
            double seconds = (double)rule.MaxRate.RateSpanType * rule.MaxRate.RateSpan;
            resp = _requestTrackingService.GetTrackedItemsInfo(
                new GetTrackedItemsRequest()
                {
                    Key = key,
                    Start = dateTime.AddSeconds(-1 *seconds),
                    End = dateTime

                });
          
            if (resp.Count >= rule.MaxRate.Rate)
            {
                return true;
            }
        }

        if (rule.VelocityRate != null)
        {
            
            double seconds = (double)rule.VelocityRate.RateSpanType * rule.VelocityRate.RateSpan;
            DateTime lastRequestDateTimeUtc;
            if (resp == null)
            {
                var lastResp = _requestTrackingService.GetLastTrackedDateTimeUtc(new GetLastTrackedDateTimeUtcRequest()
                {
                    Key = key
                });
                lastRequestDateTimeUtc = lastResp.LastTrackedDateTimeUtc;
            }
            else
            {
                lastRequestDateTimeUtc = resp.LastTrackedDateTimeUtc;
            }
            if (lastRequestDateTimeUtc.AddSeconds(seconds) > dateTime) 
            {
                return  true;
            }
        }
        return false;
    }

    private void AddRequestTracking(RateTimeRule rule, RateLimiterRequest request)
    {
        double expirySec = (int)rule.RateSpanType * rule.RateSpan;
        string trackingId = GetTrackingId(request);
        AddTrackedItemRequest addTrackingRequest = new AddTrackedItemRequest() { ExpireAfterSeconds = expirySec, Request = request, TrackingId = trackingId};
        _requestTrackingService.AddTrackedItem(addTrackingRequest);
    }

    private string GetTrackingId(RateLimiterRequest request)
    {
        return $"request_{request.ClientApplicationEndpoint.ClientApplicationEndpointId}";
    }
    private async Task<(RateLimiterRule? Rule, GetRulesRequest Request, GetRulesResponse Response)> GetRuleAsync(string ruleFile, string workflow, RateLimiterRequest request, int iteration = 0)
    {
        iteration++;
        GetRulesRequest rulesRequest = new GetRulesRequest()
        {
            RulesFile = ruleFile,
            Workflow = workflow,
            Input = new object[] { request }
        };

        var rulesResponse = await _ruleService.GetRulesAsync(rulesRequest);

        if (rulesResponse.ResponseCode != RulesService.Models.Enums.RulesServiceResponseCodeEnum.Success)
        {
            return (null, rulesRequest, rulesResponse);
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
                    if (String.IsNullOrEmpty(rateRule.Name))
                    {
                        rateRule.Name = result.RuleName;
                    }
                    rules.Add(rateRule);
                }

            }
        }

        if (rules.Count == 0)
        {
            return (null, rulesRequest, rulesResponse);
        }

        var sortedRules = rules.OrderByDescending(x => x.Priority).ToList();
        if (sortedRules[0].NextRuleFile != null && sortedRules[0].NextWorkflow != null)
        {
            if (iteration <= ConstMaxRecursiveness)
            {
                var rule = await GetRuleAsync(sortedRules[0].NextRuleFile!, sortedRules[0].NextWorkflow!, request, iteration);
                return rule;
            }
            else
            {
                rulesResponse.ResponseCode = RulesService.Models.Enums.RulesServiceResponseCodeEnum.WorkflowError;
                rulesResponse.ResponseMessage = $"Exceeded max attempts to get next config ile.";
                return (null, rulesRequest, rulesResponse);
            }
        }
        else
        {
            return (sortedRules[0], rulesRequest, rulesResponse);
        }
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
}
