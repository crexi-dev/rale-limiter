
using RateLimiter.Infrastructure;

namespace WebAPI.Infrastructure;

public interface IProductRepository<T,TU> where T : class, IIdentifiable<TU>
{
    Task<List<T>> GetAll();
    T GetById(TU id);
    T AddProduct(T product);
    T UpdateProduct(T product);
    void RemoveProduct(TU productId);
}

