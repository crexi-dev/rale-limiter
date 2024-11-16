namespace Crexi.Models;

public class ClientApplication
{
    public Guid ClientApplicationId { get; set; }
    public Guid ClientId { get; set; }
    public Guid ApplicationId { get; set; }
    public string? AuthToken { get; set; }
    public DateTime TokenExpiration { get; set; }

 
}


