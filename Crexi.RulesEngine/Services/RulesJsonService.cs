using Crexi.RulesService.Interfaces;
using Crexi.RulesService.Models;
using Crexi.RulesService.Models.Requests;
using RulesEngine.Models;
using System.Text.Json;


namespace Crexi.RulesService.Services;

public class RulesJsonService : IRulesService
{
    public async Task<BaseResponse> GetRulesAsync(BaseRequest request)
    {
        BaseResponse response = new BaseResponse();
        try
        {
            var workflows = GetWorkflows(request.RuleFile);
            if (workflows == null)
            {
                response.ResponseCode = Models.Enums.ResponseCodeEnum.WorkflowNotFound;
                response.ResponseMessage = $"Cannot find {request.RuleFile}";
                return response;
            }

            var rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray(), null);
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

    private List<Workflow>? GetWorkflows(string jsonfile)
    {

        var fileData = File.ReadAllText(jsonfile);
        var workflow = JsonSerializer.Deserialize<List<Workflow>>(fileData);
        return workflow;
    }
}
