using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class NewsController() : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetNews(string token)
    {
        return Task.FromResult<IActionResult>(Ok(new { Headline = "Breaking News", Content = "Here we go again..." }));
    }
}
