﻿using Crexi.Cache.Models;
using Crexi.Cache.Providers;
using Crexi.RulesService.Interfaces;
using Crexi.RulesService.Models;
using Crexi.RulesService.Models.Requests;
using RulesEngine.Models;
using System.Text.Json;


namespace Crexi.RulesService.Services;

public class RulesJsonService : IRulesService
{
    private readonly ICacheProvider _cacheProvider;
    private readonly CacheOptions _cacheOptions;
    public const double ConstExpiryTTLSeconds = 600;
    public readonly JsonSerializerOptions _jsonSerializerOptions;
    public RulesJsonService(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
        _cacheOptions = new CacheOptions() { CacheExpiryOption = CacheExpiryOptionEnum.Absolute, ExpiryTTLSeconds = ConstExpiryTTLSeconds };
        _jsonSerializerOptions = new JsonSerializerOptions() 
        { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
          PropertyNameCaseInsensitive = true};
    }
    public async Task<GetRulesResponse> GetRulesAsync(GetRulesRequest request)
    {
        GetRulesResponse response = new GetRulesResponse();
        try
        {
            var workflows = await GetWorkflows(request.RulesFile);
            if (workflows == null)
            {
                response.ResponseCode = Models.Enums.ResponseCodeEnum.WorkflowNotFound;
                response.ResponseMessage = $"Cannot find {request.RulesFile}";
                return response;
            }

            var rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray());

            List<RuleResultTree> resultList = await rulesEngine.ExecuteAllRulesAsync(request.Workflow, request.Input);
            for (int i = 0; i < resultList.Count; i++) 
            { 
              var rule = resultList[i]; 
              RulesResult result = new RulesResult();
              result.ErrorMessage = rule.ExceptionMessage;
              result.SuccessEvent = rule.Rule.SuccessEvent;
              result.RuleName = rule.Rule.RuleName;
              result.IsSuccess = rule.IsSuccess;
              result.Enabled = rule.Rule.Enabled;
              result.Properties = rule.Rule.Properties;
              response.RulesResults.Add(result);
            }
        }
        catch (Exception ex) 
        {
            response.ResponseCode = Models.Enums.ResponseCodeEnum.SystemError;
            response.ResponseMessage = ex.Message;
        }
        return response;
    }

    private async Task<List<Workflow>?> GetWorkflows(string jsonfile)
    {
        var workflow = await _cacheProvider.Get<List<Workflow>>(jsonfile);
        if (workflow != null) 
        {
            return workflow;
        }
        var fileData = File.ReadAllText(jsonfile);
        workflow = JsonSerializer.Deserialize<List<Workflow>>(fileData, _jsonSerializerOptions);
        if (workflow != null) {
            await _cacheProvider.Set<List<Workflow>>(jsonfile, workflow, _cacheOptions);
        }
        return workflow;
    }
}
