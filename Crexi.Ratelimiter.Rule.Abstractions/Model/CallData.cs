namespace Crexi.RateLimiter.Rule.Model;

public class CallData
{
    public int? RegionId { get; set; }
    public int? TierId { get; set; }
    public int? ClientId { get; set; }
    public required string Resource { get; set; }
}