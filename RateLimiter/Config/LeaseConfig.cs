using System.ComponentModel.DataAnnotations;

namespace RateLimiter.Config;

/// <summary>
/// This class represents the configuration for a limiter lease.
/// It should be extended with new lease properties as new limiters are defined.
/// </summary>
public class LeaseConfig
{
    /// <summary>
    /// Gets or sets the name of the resource associated with the limiter lease.
    /// </summary>
    [Required]
    public string ResourceName { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens associated with the lease.
    /// </summary>
    public int? Tokens { get; set; }
}

