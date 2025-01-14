using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RateLimiter.Attributes
{
    public class ContentResult : IActionResult
    {
        public int StatusCodes { get; set; }
        public string Content { get; set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = StatusCodes;

            context.HttpContext.Response.ContentType = "text/plain";

            if (!string.IsNullOrEmpty(Content))
            {
                await context.HttpContext.Response.WriteAsync(Content);
            }
            else
            {
                await context.HttpContext.Response.WriteAsync(string.Empty);
            }
        }
    }
}