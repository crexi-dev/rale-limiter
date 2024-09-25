using RateLimiter.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure;
using WebAPI.Models;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WidgetController : ControllerBase
{
    private readonly IProductInventoryManager<Widget, Guid> _widgetManager;

    public WidgetController(IProductInventoryManager<Widget, Guid> widgetManager)
    {
        _widgetManager = widgetManager;
    }
    // GET: api/<WidgetController>
  
    [ServiceFilter(typeof(RateLimiterAttribute))]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _widgetManager.GetInventory());
    }

    // GET api/<WidgetController>/5
    [HttpGet("{id:guid}")]
    public Widget Get(Guid id)
    {
        return _widgetManager.GetItem(id);
    }

    // POST api/<WidgetController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<WidgetController>/5
    [HttpPut("{id:guid}")]
    public void Put(Guid id, [FromBody] string value)
    {
    }

    // DELETE api/<WidgetController>/5
    [HttpDelete("{id:guid}")]
    public void Delete(Guid id)
    {
    }
}