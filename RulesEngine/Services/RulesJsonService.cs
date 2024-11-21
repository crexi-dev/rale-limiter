using Cache.Models;
using Cache.Providers;
using RulesService.Interfaces;
using RulesService.Models;
using RulesService.Models.Requests;
using RulesEngine.Models;
using System.Text.Json;
using RulesEngine.Exceptions;
using RulesService.Models.Enums;

namespace RulesService.Services;


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
        { 
          NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
          PropertyNameCaseInsensitive = true
        };
    }
    public async Task<GetRulesResponse> GetRulesAsync(GetRulesRequest request)
    {
        GetRulesResponse response = new GetRulesResponse();
        try
        {
            var workflows = await GetWorkflows(request.RulesFile);
            if (workflows == null)
            {
                response.ResponseCode = Models.Enums.RulesServiceResponseCodeEnum.WorkflowError;
                response.ResponseMessage = $"Cannot workflows from the rules file {request.RulesFile}";
                return response;
            }
            var reSettings = new ReSettings
            {
                CustomTypes = new Type[] { typeof(CustomTypes) }
            };

            var rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray(), reSettings);

            List<RuleResultTree> resultList = await rulesEngine.ExecuteAllRulesAsync(request.Workflow, request.Input);

            foreach(var rule in resultList)
            {
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
      
        catch (RuleValidationException vEx)
        {
            response.ResponseCode = RulesServiceResponseCodeEnum.ValidationError;
            response.ResponseMessage = $"Validation Exception for {request}, Exception = {vEx.Message}";
        }
        catch (ExpressionParserException pEx)
        {
            response.ResponseCode = RulesServiceResponseCodeEnum.ExpressionParserError;
            response.ResponseMessage = $"ExpressionParser Exception for {request}, Exception = {pEx.Message}";
        }
        catch (ScopedParamException rsEx)
        {
            response.ResponseCode = RulesServiceResponseCodeEnum.ScopedParameterError;
            response.ResponseMessage = $"ScopedParamException Exception for {request}, Exception = {rsEx.Message}";
        }
        catch (RuleException rEx)
        {
            response.ResponseCode = RulesServiceResponseCodeEnum.RulesEngineError;
            response.ResponseMessage = $"RuleException Exception for {request}, Exception = {rEx.Message}";
        }
        catch (Exception ex)
        {
            response.ResponseCode = RulesServiceResponseCodeEnum.SystemError;
            response.ResponseMessage = $"System Exception for {request}, Exception = {ex.Message}";
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
