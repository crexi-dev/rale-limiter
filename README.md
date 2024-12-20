# Rate Limiter

## Introduction

The Rate Limiting pattern is a technique used to control the rate at which clients can make requests to a server. This helps prevent abuse and ensures fair usage of resources. In this project, we implement a simple, reusable rate limiter class that can be configured with different rate limiting rules. The goal is to demonstrate the concept of rate limiting without the need for a complex environment or API configurations.

## Rate Limiting Algorithms (Rules)

The following rate limiting rules are implemented in the code:

- **RequestsPerTimespanRule**: Limits the number of requests a client can make within a specified timespan.
- **TimespanSinceLastCallRule**: Ensures a minimum timespan has passed since the last request from a client.
- **DailyRequestLimitRule**: Limits the number of requests a client can make in a 24-hour period.
- **ConcurrentRequestLimitRule**: Limits the number of concurrent requests a client can make.
- **BurstRequestLimitRule**: Allows a burst of requests in a short period but enforces a cooldown period afterward.
- **RegionBasedRule**: Applies different rules based on the client's region (e.g., US or EU).

## Design Patterns Used

- **Strategy Pattern**: Each rate limiting rule implements the `IRateLimitingRule` interface, allowing the `RateLimiter` class to use different strategies for rate limiting.
- **Decorator Pattern**: The `RegionBasedRule` class decorates other rules to apply different rate limiting strategies based on the client's region.
- **Repository Pattern**: A fake repository is used to determine the location of a client by their access token.

## Thread Safety

The rate limiter and its rules are designed to be thread-safe. Concurrent collections such as `ConcurrentDictionary` and `ConcurrentBag` are used to ensure safe access to shared data in a multi-threaded environment.

## Assumptions

- By default, a request is allowed if no rules are defined for the resource.

## Future Enhancements

- **Builder Pattern**: Implement a builder pattern to allow chaining of method calls to configure rules fluently.
- **Upgrade to .NET 8.0**: Upgrade the project to .NET 8.0, the latest Long-Term Support (LTS) version of .NET.
- **Response Object**: Return a helpful message or type as a response object to provide more information about the rate limiting decision.

## Usage

This project is a simple, reusable class library to demonstrate the rate limiter concept. It does not include any complex environment setups, API configurations, or web frameworks. The focus is on the design and implementation of the rate limiting rules and their integration into a rate limiter class.

## Example

Here is an example of how to use the rate limiter with different rules:

``` csharp
var clientRepository = new ClientRepository(); 
var usRule = new RequestsPerTimespanRule(10, TimeSpan.FromMinutes(1)); 
var euRule = new TimespanSinceLastCallRule(TimeSpan.FromSeconds(30)); 
var locationBasedRule = new RegionBasedRule(usRule, euRule, clientRepository);

var rateLimiter = new RateLimiter(); 
rateLimiter.AddRule("resource1", locationBasedRule);

// Check if requests are allowed 
bool isAllowed1 = rateLimiter.IsRequestAllowed("resource1", "client1"); // US-based rule 
bool isAllowed2 = rateLimiter.IsRequestAllowed("resource1", "client2"); // EU-based rule
```

## Extensibility and Configurability

The implementation is designed to be extendable and configurable, meeting the initial requirements.

### Extensibility

1. **Strategy Pattern**: Each rate limiting rule implements the `IRateLimitingRule` interface, allowing new rules to be added easily. You can create new classes that implement this interface and add them to the `RateLimiter` class.
   
2. **Decorator Pattern**: The `RegionBasedRule` class demonstrates how you can combine multiple rules and apply them based on specific conditions (e.g., client region). This pattern can be extended to create more complex combinations of rules.

3. **Concurrent Collections**: The use of thread-safe collections like `ConcurrentDictionary` and `ConcurrentBag` ensures that the implementation can be safely extended to handle more complex scenarios without running into concurrency issues.

### Configurability

1. **AddRule Method**: The `RateLimiter` class provides an `AddRule` method that allows you to add multiple rules for different resources. This makes it easy to configure the rate limiter for various API endpoints.

2. **Flexible Rule Combinations**: You can combine different rules for a single resource. For example, you can use both `RequestsPerTimespanRule` and `TimespanSinceLastCallRule` for a single resource, ensuring that both conditions must be met for a request to be allowed.

3. **Fake Repository**: The `ClientRepository` class is used to determine the client's region based on their access token. This can be easily replaced or extended to use a real database or external service for more complex configurations.

### Example of Extensibility and Configurability

Here is an example demonstrating how you can extend and configure the rate limiter:

``` csharp
// Define a new rule 
public class CustomRateLimitRule : IRateLimitingRule
{
    private readonly int _maxRequests; 
    private readonly TimeSpan _timespan; 
    private readonly ConcurrentDictionary<string, List> _requestLog = new();
    public CustomRateLimitRule(int maxRequests, TimeSpan timespan)
    {
        _maxRequests = maxRequests;
        _timespan = timespan;
    }

    public bool IsRequestAllowed(string clientId)
    {
        // Rule logic here
    }
}

// Configure the rate limiter 
var rateLimiter = new RateLimiter(); 
var customRule = new CustomRateLimitRule(10, TimeSpan.FromMinutes(1)); 
rateLimiter.AddRule("resource1", customRule); 
rateLimiter.AddRule("resource1", new TimespanSinceLastCallRule(TimeSpan.FromSeconds(30)));

// Check if requests are allowed 
bool isAllowed = rateLimiter.IsRequestAllowed("resource1", "client1");
```

## Original task description
**Rate-limiting pattern**

Rate limiting involves restricting the number of requests that a client can make.
A client is identified with an access token, which is used for every request to a resource.
To prevent abuse of the server, APIs enforce rate-limiting techniques.
The rate-limiting application can decide whether to allow the request based on the client.
The client makes an API call to a particular resource; the server checks whether the request for this client is within the limit.
If the request is within the limit, then the request goes through.
Otherwise, the API call is restricted.

Some examples of request-limiting rules (you could imagine any others)
* X requests per timespan;
* a certain timespan has passed since the last call;
* For US-based tokens, we use X requests per timespan; for EU-based tokens, a certain timespan has passed since the last call.

The goal is to design a class(-es) that manages each API resource's rate limits by a set of provided *configurable and extendable* rules. For example, for one resource, you could configure the limiter to use Rule A; for another one - Rule B; for a third one - both A + B, etc. Any combination of rules should be possible; keep this fact in mind when designing the classes.

We're more interested in the design itself than in some intelligent and tricky rate-limiting algorithm. There is no need to use a database (in-memory storage is fine) or any web framework. Do not waste time on preparing complex environment, reusable class library covered by a set of tests is more than enough.

There is a Test Project set up for you to use. However, you are welcome to create your own test project and use whatever test runner you like.   

You are welcome to ask any questions regarding the requirements—treat us as product owners, analysts, or whoever knows the business.
If you have any questions or concerns, please submit them as a [GitHub issue](https://github.com/crexi-dev/rate-limiter/issues).

You should [fork](https://help.github.com/en/github/getting-started-with-github/fork-a-repo) the project and [create a pull request](https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/creating-a-pull-request-from-a-fork) named as `FirstName LastName` once you are finished.

Good luck!
