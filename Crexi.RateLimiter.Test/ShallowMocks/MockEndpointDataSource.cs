using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Crexi.RateLimiter.Test.ShallowMocks;

public class MockEndpointDataSource: EndpointDataSource
{
    public override IChangeToken GetChangeToken()
    {
        throw new NotImplementedException();
    }

    public override IReadOnlyList<Endpoint> Endpoints =>
    [
        new(null, null, "HTTP: GET /test/{id}"),
        new(null, null, "HTTP: POST /test"),
        new(null, null, "HTTP: PUT /test"),
        new(null, null, "HTTP: DELETE /test/{id}"),
    ];
}