namespace WebAPI.Infrastructure;

public interface IProductInventoryManager<T, TU>
{
    Task<List<T>> GetInventory();
    T GetItem(TU id);
}