using System.Collections.Concurrent;

namespace Services.Common.Repositories;

public class InMemoryDataRepository<T> : IDataRepository<T>
{
    private readonly ConcurrentBag<T> _items;
    
    public InMemoryDataRepository(ConcurrentBag<T> items)
    {
        _items = items;
    }
    
    public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult((IEnumerable<T>)_items);

    public Task<T> GetByIdAsync(Guid id)
    {
        var propertyInfo = typeof(T).GetProperty("Id");
        return Task.FromResult(_items.FirstOrDefault(e => (Guid)(propertyInfo?.GetValue(e) ?? Guid.Empty) == id));
    }
}