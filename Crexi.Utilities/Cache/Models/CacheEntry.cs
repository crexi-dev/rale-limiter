using System.Text.Json.Serialization;


namespace Crexi.Cache.Models;

public class CacheEntry<T>
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public T Value { get; set; }
    public CacheOptions Options { get; set; }
 
    [JsonConstructor]
     public CacheEntry(Guid id, string key, T value, CacheOptions options)
    {
        Id = id;
        Options = options;
        Key = key;
        Value = value;
    }
    public CacheEntry(string key, T value)
    {
        Id = Guid.NewGuid();
        Options = new CacheOptions();
        Key = key;
        Value = value;

    }

}
