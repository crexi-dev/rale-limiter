# Rate Limiting Library & Web API Integration

This repository showcases a comprehensive rate limiting library integrated with a .NET Core Web API project. The solution demonstrates various rate limiting strategies, composite strategies, region-specific rules, and a global catch-all rate limiter. It also illustrates how to apply these strategies using custom attributes on API endpoints, facilitating easy configuration for different resources. The WebAPI created is purely for demonstrative purposes to show how one would typically interface with the Rate Limting liberary via such attributes, making it easy to decorate and restrict endpoints on your API controller action methods on a per-resource basis.

---

## Overview

The project comprises the following components:

- **RateLimiter Library**: Implements diverse rate limiting algorithms and design patterns.
- **Custom Attributes**: ASP.NET Core action filters to enforce rate limits on Web API endpoints.
- **Web API**: Sample controllers and endpoints that demonstrate real-world usage scenarios.
- **Unit Tests**: NUnit tests that validate the logic and functionality of the rate limiting library and attributes.

---

## Architecture

### RateLimiter Library

The library leverages the **Strategy** and **Composite** design patterns to support multiple rate limiting rules:

- **Strategies**:
  - **FixedWindowRule**: Limits the number of requests per fixed time window for a specific client.
  - **CooldownRule**: Ensures a minimum time interval between consecutive requests from the same client.
  - **GlobalFixedWindowRule**: Applies a global rate limit across all clients, regardless of their tokens.
  - **CompositeRateLimitStrategy**: Combines multiple strategies, enforcing all of them simultaneously for a request to pass.

Each strategy interacts with an **IUsageRepository** (e.g., `InMemoryUsageRepository`) to store and retrieve usage data.

### Custom Attributes

Custom attributes implement `IAsyncActionFilter` to apply rate limiting at the controller or action level:

- **RateLimitAttribute**: Applies a single rate limiting strategy to an endpoint.
- **RegionRateLimitAttribute**: Enforces different strategies based on the client's region (e.g., US vs. EU). Note: a cloud service provider would normally inject region metadata in the headers, here however I am assuming "US-" and "EU-" for demonstration purposes, but in the real world you would filter against whatever the cloud provider delivers.
- **CompositeRateLimitAttribute**: Applies a combination of multiple rate limiting strategies to an endpoint.
- **GlobalRateLimitAttribute**: Enforces a global rate limit, independent of client tokens.

These attributes intercept incoming requests, evaluate them against the configured rate limiting rules, and return appropriate HTTP responses if limits are exceeded.

### How It Works

1. **Endpoint Decoration**: API endpoints are decorated with custom attributes specifying their rate limiting configuration.
2. **Attribute Execution**: When a request targets an endpoint:
   - The corresponding attribute's filter logic is executed.
   - It retrieves necessary identifiers (e.g., client tokens) and selects the appropriate rate limiting strategy.
   - The strategy evaluates whether the request should be allowed or blocked based on current usage and configured limits.
   - If the request exceeds the limit, an HTTP 429 (Too Many Requests) response is returned.
   - Otherwise, the request proceeds to the controller action.

3. **State Management**: Rate limiting strategies use repositories and shared/static variables to maintain state across requests, ensuring consistent enforcement of limits.

---

## Endpoints Demonstration

### Example Endpoints

- **Regional Listings**: Uses `RegionRateLimitAttribute` to apply different rate limiting rules for US and EU clients.
- **Composite Listings**: Uses `CompositeRateLimitAttribute` to enforce a combination of rate limiting rules (e.g., fixed window plus cooldown).
- **Global Listings**: Uses `GlobalRateLimitAttribute` to enforce a global rate limit across all clients, irrespective of their tokens.

### Using Postman for Testing

1. **Set Headers**: Include headers like `X-Client-Token` to identify clients.
2. **Issue Requests**: Rapidly send requests to different endpoints to observe how rate limits are enforced.
3. **Change Token Prefixes**: Use different token prefixes (e.g., `US-`, `EU-`) to test region-specific behaviors.
4. **Test Global Limits**: Access global endpoints to verify catch-all rate limiting irrespective of client tokens.

---

## Running Tests

The solution includes comprehensive NUnit tests covering:

- **Individual Strategies**: Ensuring each rate limiting strategy (`FixedWindowRule`, `CooldownRule`, `GlobalFixedWindowRule`) behaves as expected.
- **Composite Strategies**: Validating that combined strategies enforce all rules correctly.
- **Attribute Behavior**: Testing custom attributes to ensure they correctly apply rate limiting rules based on configurations and client regions.
- **Global Rate Limiting**: Confirming that global limits are enforced across all requests regardless of client identity.

### How to Run Tests

1. **Build the Solution**: Ensure the project builds successfully.
2. **Execute Tests**: Use your preferred test runner (e.g., Visual Studio Test Explorer, `dotnet test`) to run the NUnit test suite.
3. **Review Results**: Verify that all tests pass, ensuring the rate limiting logic is functioning correctly.

---

## Extending the Library

The rate limiting library is designed for flexibility and ease of extension:

- **Adding New Strategies**: Implement the `IRateLimitStrategy` interface to introduce new rate limiting algorithms.
- **Creating Custom Attributes**: Develop new attributes that leverage existing or new strategies to apply rate limits to different endpoints.
- **Configuration Flexibility**: Adjust attribute parameters to fine-tune rate limits without modifying core library logic.
- **Integrating with Different Repositories**: Switch out the `IUsageRepository` implementation (e.g., using a distributed cache) to scale the rate limiter for different environments.

---
