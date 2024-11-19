using System.Xml.Linq;

namespace Data.Models;

public class ClientApplication
{
    public Guid ClientApplicationId { get; set; }
    public Guid ClientId { get; set; }
    public Guid ApplicationId { get; set; }
    public string? AuthToken { get; set; }
    public DateTime TokenExpiration { get; set; }
    public string? RulesConfigFileOverride { get; set; }
    public string? RulesWorkflowOverride { get; set; }

    public override string ToString()
    {
        return $"ClientId : {ClientId}, ClientApplicationId : {ClientApplicationId}, RulesConfigFileOverride : {RulesConfigFileOverride}, RulesWorkflowOverride : {RulesWorkflowOverride}";
    }
}


