
namespace Cache.Models;

public enum CacheExpiryOptionEnum : int
{
    Absolute,
    Sliding
}
public class CacheOptions
{
    public CacheExpiryOptionEnum CacheExpiryOption { get; set; }
    public double ExpiryTTLSeconds {get;set;}

}
