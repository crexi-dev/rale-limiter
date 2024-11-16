using RateLimiter.Models.Requests;
using System;
using System.Collections.Generic;


namespace RateLimiter.Tests;

public class TestData
{

    public static List<RateLimiterRequest> GetUSRequest()
    {
        RateLimiterRequest request = new RateLimiterRequest();
        request.Client = new Crexi.Models.Client()
        {
            ClientId = new Guid("F57B1BB8-CB73-42AD-935F-2352F491ABC0"),
            DefaultCountryCode = "US",
            Tier = "Tier10"

        };
        request.Endpoint = new Crexi.Models.Endpoint()
        {
            EndpointId = new Guid("230bfb42-594d-4171-a925-e127f90f4b28")
        };

        request.Application = new Crexi.Models.Application()
        {
            ApplicationId = new Guid("5331562b-255c-4b55-8930-1730c8c60828")
        };

        return new List<RateLimiterRequest>() { request };
    }
}
