using RateLimiter.Models.Requests;
using System;
using System.Collections.Generic;

using Data.Models;
namespace RateLimiter.Tests;

public class TestData
{
    public const string ConstUSClientId = "fa2d5de9-315f-4e93-9bf1-9c098e84bd21";
    public const string ConstApplicationId = "1046073e-1350-48ba-a4dd-923df6fc8ecc";
    public const string ConstEndpointId = "c370ed23-9366-45d4-b05b-95e5fab451a0";
    public const string ConstUSClientApplicationId = "143afc7a-e964-4d7b-b03a-3120ac500ca5";
   

    public const string ConstEUClientId = "883135fc-8605-4665-b324-15cc956967cd";
    public const string ConstEUClientApplicationId = "2c17bafb-00a6-434e-9d55-4cd7af5f8684";
    public const string ConstEUClientApplicationEndpointId = "9759c17e-5cec-42ae-bd44-388fc294578b";

    public static RateLimiterRequest GetUSClientRequest(Guid clientApplicationEndpointId)
    {
        RateLimiterRequest request = new RateLimiterRequest();
        request.Client = new Client()
        {
            ClientId = new Guid(ConstUSClientId),
            DefaultCountryCode = "US",
            DefaultRegionCode = "US",

        };
        request.ClientApplicationEndpoint = new ClientApplicationEndpoint()
        {
            ClientApplicationEndpointId = clientApplicationEndpointId,
            EndpointId = new Guid(ConstEndpointId),
            ClientApplicationId = new Guid(ConstUSClientApplicationId),
        };

        request.ClientApplication = new ClientApplication()
        {
            ApplicationId = new Guid(ConstApplicationId),
            ClientId = new Guid(ConstUSClientId),
            ClientApplicationId = new Guid(ConstUSClientApplicationId),
        };

        return request;
    }

    public static RateLimiterRequest GetEUClientRequest()
    {
        RateLimiterRequest request = new RateLimiterRequest();
        request.Client = new Client()
        {
            ClientId = new Guid(ConstEUClientId),
            DefaultCountryCode = "DE",
            DefaultRegionCode = "EU"

        };
        request.ClientApplicationEndpoint = new ClientApplicationEndpoint()
        {
            ClientApplicationEndpointId = new Guid(ConstEUClientApplicationEndpointId),
            EndpointId = new Guid(ConstEUClientApplicationEndpointId),
            ClientApplicationId = new Guid(ConstEUClientApplicationId),
        };

        request.ClientApplication = new ClientApplication()
        {
            ApplicationId = new Guid(ConstApplicationId),
            ClientId = new Guid(ConstEUClientId),
            ClientApplicationId = new Guid(ConstEUClientApplicationId),
        };

        return request;
    }
}
