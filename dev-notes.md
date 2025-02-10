Top-down approach?  If this rate limiter were a nuget pkg that I was going to utilize, how would I imagine I would configure/use it?

What would an endpoint look like?

```
public class ResourceA
{
	[RateLimit("clientToken", Strategy.RequestsPerTimespan)]
	[HttpGet()]
	public async Task<IActionResult> GetResources(string id) {
	   // ... implementation
	}

	[RateLimit("clientToken", Strategy.GeoBased)]
	[RateLimit("clientToken", Strategy.RequestsPerTimespan)]
	[HttpGet("{id}")]
	public async Task<IActionResult> GetResourceById(string id) {
	   // ... implementation
	}
}
```

hmm.  what about for minimal apis?  the library should support both implementation.

```
app.MapGet("/get", context -> context.Response.WriteAsync("get")).RateLimit(clientToke, Strategy.RequestsPerTimespan);
```

I think something like that would be acceptable.
