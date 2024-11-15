namespace Crexi.Models;

public class Client
{
    public Guid ClientId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CountryCode { get; set; }
    public string? DefaultZipCode { get; set; }
    public string? DefaultStateCode { get; set; }
    public string? DefaultRegion { get; set; } 
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public ClientAttributes? ClientAttributes { get; set; }
    

}
