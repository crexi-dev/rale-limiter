# DEV Notes

## Project Structure

- **RateLimiter Solution**
  - **RateLimiter.Api**
  - **RateLimiter**
    - Class library containing attributes and rules, as well as some additional contract interfaces like `IGeoService` and `IRateLimitStorage`.
  - **RateLimiter.Tests**
    - **Integration**
      - API Tests
    - **Unit**
      - Middleware Tests
      - Rules Tests
      - Storage Tests

In a real-world scenario, I would have broken the solution into multiple projects as shown below, so that each project builds on top of the others. Furthermore, we could package the solution into multiple NuGet packages that could be easily imported into your solution according to the type of project you are creating, storage you want to use, etc..

- **RateLimiter Solution**
  - **RateLimiter.Api**
    - Just the API project with controllers
  - **RateLimiter.AspNetCore**
    - Middleware implementation, and possibly any extensions to help with the integration with ASP.NET Core
  - **RateLimiter.GeoRules**
    - Geo Rule implementation
  - **RateLimiter.Storage**
    - Storage implementation
  - **RateLimiter.Core**
    - Attributes, basic rules and contract interfaces    
  - **RateLimiter.Integration.Tests**
    - API Tests
  - **RateLimiter.AspNetCore.Tests**
    - Middleware Tests
  - **RateLimiter.GeoRules.Tests**
    - Geo Rule Tests
  - **RateLimiter.Storage.Tests**
    - In-Memory Storage Tests
  - **RateLimiter.Core.Tests**
    - Core Tests

## Components

### Simple Geo Service

This service is a simple implementation that serves as a helper to demonstrate the concept. It only considers the two examples given in the exercise (US, EU).

### Middleware

To facilitate the application of rate limiting and abstract it away from the controllers, a middleware was created. This middleware is responsible for checking the rate limits and returning the appropriate responses. While it performs some authentication verification, it's only an implementation detail since we are using a simple token for rate limiting. A more complex authentication mechanism could be implemented in the future, and features like per-customer rate limiting could be added.

## Testing

The tests are simple and cover the basic functionality of the rate limiting. They are not exhaustive but are a good starting point. Additional tests that would cover edge cases could be added to make the solution more robust, but in order to maintain the time box that I set for myself, those were left out. The `SimpleGeoService` was left untested since it is a simple implementation and the tests would be redundant.

## Suggestions

### .NET Version Upgrade

As of November 12th, 2024, .NET 6 will be officially out of support. I would recommend updating the projects to the latest version of .NET.
