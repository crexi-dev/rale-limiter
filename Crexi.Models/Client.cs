namespace Crexi.Models;

public class Client
{
    public Guid ClientId { get; set; }
    public string SecretHash { get; set; } = null!;
    public string SecretSalt { get; set; } = null!;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? DefaultCountryCode { get; set; }
    public string? DefaultZipCode { get; set; }
    public string? DefaultStateCode { get; set; }
    public string? DefaultRegion { get; set; }
    public string Tier { get; set; } = "Default";
    public string Classification { get; set; } = "Default";
    public double AverageTransactionVolume { get; set; }

}
