using Cache.Providers;
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

            if (ruleResponse.Response.ResponseCode != RulesService.Models.Enums.RulesServiceResponseCodeEnum.Success) 
            {
                response.ResponseCode = Models.Enums.ResponseCodeEnum.RulesEngineError;
              
                response.ResponseMessage = ruleResponse.Response.ResponseMessage;
                _logger.LogWarning($"{response}. {trace}");
                return response;
            }

            if (ruleResponse.Rule == null)
            {
                response.ResponseMessage = $"Limitation Rule is not found. {trace}. {ruleResponse.Response}";
                _logger.LogWarning($"{response}. {trace}");
                return response;
            }
            response.RateLimiterRule = ruleResponse.Rule;
            var rateValidationResult = await IsRequestRateExceeded(ruleResponse.Rule.MaxRate, request, ConstCachePrefixMaxRate);

            response.CurrentRate = rateValidationResult.CurrentCount;
            if (rateValidationResult.IsExceeded)
            {
                response.IsRateExceeded = true;
                response.ResponseMessage = "Request max rate is exceeded.";
                _logger.LogWarning($"{response}.{trace}");
                return response;
            }
            
            rateValidationResult = await IsRequestRateExceeded(ruleResponse.Rule.VelocityRate, request, ConstCachePrefixVelocityRate);
            response.CurrentVelocityRate = rateValidationResult.CurrentCount;
            if (rateValidationResult.IsExceeded)
            {
                response.IsRateExceeded = true;
                response.ResponseMessage = "Request velocity rate is exceeded.";
                _logger.LogWarning($"{response}. {trace}");
                return response;
            }

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
    private async Task<(bool IsExceeded, double CurrentCount)> IsRequestRateExceeded(RateTimeRule? rule, RateLimiterRequest request, string prefix)
    {
        if (rule == null)
        { return (true, 0); }

        GetByPatternResponse resp = await _requestTrackingService.GetTrackingResponseAsync(new GetByPatternRequest() { RequestIdPattern = GetTrackingId(request, prefix) });
        if(resp.TrackingItems == null)
        {
            return (true, 0);
        }

        double count = resp.TrackingItems.Where(x => x.UtcDateTime.AddSeconds((double)rule.RateSpanType * rule.RateSpan * rule.Rate) > DateTime.UtcNow).Count();
        if (count < rule.Rate)
        {
            return (false, count);
        }
        return (true, count); ;
    }

    public async Task AddRequestTrackingAsync(RateTimeRule? rule, RateLimiterRequest request, string prefix)
    {
        if (rule == null)
        { return; }
        double expirySec = (int)rule.RateSpanType * rule.RateSpan;
        string trackingId = GetTrackingId(request, prefix);
        AddTrackingRequest addTrackingRequest = new AddTrackingRequest() { ExpireAfterSeconds = expirySec, Request = request, TrackingId = trackingId};
        await _requestTrackingService.AddTrackingAsync(addTrackingRequest);
    }
    private string GetTrackingId(RateLimiterRequest request, string prefix)
    {
        return $"{prefix}_{request.ClientApplicationEndpoint.ClientApplicationEndpointId}";
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

    public string? GetObjectValue(object? obj)
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
