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



# Rate Limiter Library

## Overview
The **Rate Limiter Library** implements a rate-limiting pattern to manage API request limits for different clients. By providing a set of configurable and extendable rules, the library allows developers to control access to API resources effectively, ensuring compliance with rate limits and preventing server abuse.

### Features
- Configurable rules for rate-limiting (e.g., X requests per timespan, a delay between requests).
- Extendable design to add new rule types or combinations.
- In-memory storage for rule tracking, avoiding external database dependencies.
- Unit-tested for robustness and reliability.

---

## Key Concepts

### Rate Limiting
Rate limiting restricts the number of requests that a client can make within a specific timeframe. Each client is identified by an access token used for all requests.

When a request is received:
1. The library determines if the request complies with the rate-limiting rules.
2. If within limits, the request is processed.
3. If the limit is exceeded, the request is rejected.

### Examples of Rules
1. **X requests per timespan**: Allow up to N requests within a specified time window.
2. **Delay between requests**: Enforce a minimum delay between consecutive requests.
3. **Region-specific rules**: Apply different rules based on client location (e.g., stricter rules for US-based tokens).
4. **Combinations**: Combine multiple rules for a single resource.

---

## Library Design
The library is designed with flexibility and configurability as primary goals.

### Core Components
1. **Rule Interface**
   - Defines the contract for all rate-limiting rules.
   - Example: `bool IsRequestAllowed(ClientContext context)`

2. **Predefined Rules**
   - **FixedWindowRule**: Limits X requests within a fixed timespan.
   - **SlidingWindowRule**: Limits requests dynamically based on a sliding window.
   - **CooldownRule**: Ensures a delay between consecutive requests.

3. **RateLimiter**
   - Orchestrates rule evaluation for API resources.
   - Supports applying multiple rules to a single resource.

4. **ClientContext**
   - Represents the client details (e.g., access token, region).
   - Passed to rules for evaluation.

5. **In-Memory Storage**
   - Tracks request metadata to evaluate rules.

---

## Usage

### Setting Up
1. Add the library to your project.
2. Create an instance of `RateLimiter`.
3. Configure rules for each API resource.

### Example
```csharp
// Create a RateLimiter instance
var rateLimiter = new RateLimiter();

// Define rules
var ruleA = new FixedWindowRule(maxRequests: 10, timeSpan: TimeSpan.FromMinutes(1));
var ruleB = new CooldownRule(delay: TimeSpan.FromSeconds(5));

// Associate rules with a resource
rateLimiter.AddRules("/api/resource", new[] { ruleA, ruleB });

// Check a request
var clientContext = new ClientContext("clientToken", "US");
if (rateLimiter.IsRequestAllowed("/api/resource", clientContext)) {
    Console.WriteLine("Request allowed.");
} else {
    Console.WriteLine("Request denied.");
}
```

---





