using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Infrastructure;

public interface ICacheRepository<in TKey, TValue, TReturnType>
{
    Task<TReturnType?> Get(TKey key);
    
    Task<IEnumerable<TValue>?> GetMany(TKey key);

    Task Add(TKey key, TValue value);
    
    Task Update(TKey key, TValue value);
    
    Task Delete(TKey key);
}