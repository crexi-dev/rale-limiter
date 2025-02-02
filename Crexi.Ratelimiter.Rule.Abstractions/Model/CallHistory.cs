namespace Crexi.RateLimiter.Rule.Model;

public class CallHistory
{
    public DateTime[]? Calls { get; set; }
    public DateTime? LastCall { get; set; }
}