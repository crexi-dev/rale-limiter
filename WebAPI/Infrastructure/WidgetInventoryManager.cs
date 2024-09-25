
using WebAPI.Models;

namespace WebAPI.Infrastructure;

public class WidgetInventoryManager(ILogger<WidgetInventoryManager> logger, IProductRepository<Widget, Guid> repository)
    : IProductInventoryManager<Widget, Guid>
{
    private readonly ILogger<WidgetInventoryManager> _logger = logger;

    public async Task<List<Widget>> GetInventory()
    {
        var widgets = await repository.GetAll();
        return widgets;
    }

    public Widget GetItem(Guid id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return repository.GetById(id);
    }
}