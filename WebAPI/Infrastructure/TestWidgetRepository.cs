using WebAPI.Models;

namespace WebAPI.Infrastructure;

public class TestWidgetRepository : ProductRepositoryBase<Widget, Guid>
{
    public TestWidgetRepository()
    {
        List<Widget> widgets = [
            new Widget("firstWidget", "This is the first widget."),
            new Widget("secondWidget", "This is the second widget."),
            new Widget("thirdWidget", "This is the third widget.")
        ];
        
        AddProducts(widgets);
    }
}