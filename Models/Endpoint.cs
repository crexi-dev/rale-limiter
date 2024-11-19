namespace Data.Models;

public class Endpoint
{
    public Guid EndpointId { get; set; }
    public Guid ApplicationId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string Controller { get; set; } = null!;
    public string HttpMethod { get; set; } = null!;
    public string Route { get; set; } = null!;
    public string MethodName { get; set; } = null!;

}
