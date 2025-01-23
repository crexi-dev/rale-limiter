namespace Crexi.RateLimiter.Models;

/// <summary>
/// ClientFilter points to the ClientRequest class and allows rate policies to apply and group requests
/// </summary>
public class ClientFilter
{
    /// <summary>
    /// Maps to properties in ClientRequest, examples: Region or subscription level
    /// </summary>
    public required string PropertyName { get; set; }
    
    /// <summary>
    /// Flag indicating to include/exclude a value, example: NOT in California => HasTargetValue = false 
    /// </summary>
    public required bool HasTargetValue { get; set; } 
    
    /// <summary>
    /// Target value of the filter, examples for California and British Columns: US-CA, CA-BC
    /// </summary>
    public required string TargetValue { get; set; }
}