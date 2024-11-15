
namespace Crexi.Models;

//this can be attribute(key - value) style
//these are some arbitrary style Client Metadata/Attribute
public class ClientAttributes
{
    public string Tier { get; set; } = "Default";
    
    public string Classification { get; set; } = "Default";

    public double AverageTransactionVolume { get; set; }
}
