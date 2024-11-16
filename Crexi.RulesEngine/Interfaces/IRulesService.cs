using Crexi.RulesService.Models.Requests;

namespace Crexi.RulesService.Interfaces;

public interface IRulesService
{
    Task<GetRulesResponse> GetRulesAsync(GetRulesRequest request);
}
