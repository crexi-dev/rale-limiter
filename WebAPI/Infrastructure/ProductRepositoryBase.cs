using RateLimiter.Infrastructure;

namespace WebAPI.Infrastructure;

public abstract class ProductRepositoryBase<T, TU> : IProductRepository<T, TU>
    where T : class, IIdentifiable<TU> where TU : new()
{
    private readonly List<T> _productList = [];
    private TU _id = new();

    protected virtual void AddProducts(List<T> productList)
    {
        productList.ForEach(x => AddProduct(x));
    }

    private void AddInitialProduct(T initialProduct)
    {
        ArgumentNullException.ThrowIfNull(initialProduct.Id);
        _productList.Add(initialProduct);
    }

    private static T CreateNewProduct()
    {
        return Activator.CreateInstance<T>();
    }

    public virtual Task<List<T>> GetAll()
    {
        return Task.FromResult(_productList);
    }

    public virtual T GetById(TU id)
    {
        return _productList.FirstOrDefault(p =>
            Equals((TU)typeof(T).GetProperty("Id").GetValue(p), id));
    }

    public virtual T AddProduct(T product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _productList.Add(product);
        return product;
    }

    public virtual T UpdateProduct(T product)
    {
        var productId = (TU)typeof(T).GetProperty("Id").GetValue(product);
        var existingProduct = _productList.FirstOrDefault(p =>
            Equals((TU)typeof(T).GetProperty("Id").GetValue(p), productId));

        if (existingProduct == null)
        {
            throw new ArgumentException("Product not found.");
        }

        var index = _productList.IndexOf(existingProduct);
        _productList[index] = product;

        return product;
    }

    public virtual void RemoveProduct(TU productId)
    {
        _productList.Remove(GetById(productId));
    }
}