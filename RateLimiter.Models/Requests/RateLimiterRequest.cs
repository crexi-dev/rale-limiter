using Models;

namespace RateLimiter.Models.Requests;

public class RateLimiterRequest
{
    public Client? Client { get; set; } 
    public Application? Application { get; set; }
    public Endpoint? Endpoint { get; set; }
    public RequestContext? RequestContext { get; set; }
    public DateTime? StartTime { get; set; }
    
}
