using Crexi.RulesService.Models.Requests;

namespace Crexi.RulesService.Interfaces;

internal interface IRulesService
{
    Task<BaseResponse> GetRulesAsync(BaseRequest request);
}
