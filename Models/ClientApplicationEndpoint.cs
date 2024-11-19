namespace Data.Models;

public class ClientApplicationEndpoint
{
    public Guid ClientApplicationEndpointId { get; set; }
    public Guid ClientApplicationId { get; set; }
    public Guid EndpointId { get; set; }
    public string? RulesConfigFileOverride { get; set; }
    public string? RulesWorkflowOverride { get; set; }

    public override string ToString()
    {
        return $"ClientApplicationEndpointId : {ClientApplicationEndpointId}, EndpointId : {EndpointId}, ClientApplicationId : {ClientApplicationId}, RulesConfigFileOverride : {RulesConfigFileOverride}, RulesWorkflowOverride : {RulesWorkflowOverride}";
    }
}
