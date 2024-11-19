using Data.Models;

namespace RateLimiter.Models.Requests;

public class RateLimiterRequest
{
    public Client Client { get; set; } = null!;
    public ClientApplication ClientApplication {get; set;} = null!;
    public ClientApplicationEndpoint ClientApplicationEndpoint { get; set; } = null!;

    public RequestContext? RequestContext { get; set; }
    public DateTime RequestDateTimeUTC { get; set; }
    public string? RuleConfigFile { get; set; }
    public string? RuleWorkflow { get; set; }
    public RateLimiterRequest()
    {
        RequestDateTimeUTC = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"Client : {{{Client}}}, ClientApplication : {{{ClientApplication}}}, ClientApplicationEndpoint : {{{ClientApplicationEndpoint}}}";
    }

}
