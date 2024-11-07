# Readme

## Rate-limiting pattern

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

You are welcome to ask any questions regarding the requirements�treat us as product owners, analysts, or whoever knows the business.
If you have any questions or concerns, please submit them as a [GitHub issue](https://github.com/crexi-dev/rate-limiter/issues).

You should [fork](https://help.github.com/en/github/getting-started-with-github/fork-a-repo) the project and [create a pull request](https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/creating-a-pull-request-from-a-fork) named as `FirstName LastName` once you are finished.

Good luck!

## Rate-limiting Solution

### Problem Statement:
- Design a resource rate limiting configuration framework with the following goals and non-goals:

### Goals:
- Configurability:
	- The framework should allow for the configuration of rate limiting rules on multiple resources.
- Composability:
	- The framework should be capable of applying multiple rate limiting rules to a single resource.
- Extensibility:
	- The framework should support the creation of new rate limiting rules in the future.

### Non-Goals:
- Do not create a sophisticated rate limiting algorithm, the focus is on managing rate limiting rules on specific resources.

### Implementation Details:

**Core Functionality**
The solution implements a **ResourceRateLimiter** class that receives a list of resources, along with rate limiting rules for each resource. It then utilizes the **PartitionedRateLimiter** class from the [.NET NuGet RateLimiting package](https://www.nuget.org/packages/System.Threading.RateLimiting) to orchestrate the application rules on each supplied resource.

ResourceRateLimiter supports the configuration of multiple resources with composable rate limit rules on each resource.

Each Rate Limiter class inherits from the **System.Threading.RateLimiting.RateLimiter** class, ensuring extensibility to support custom rate limiting algorithms. An example rate limiting algorithm called **RandomRateLimiter** was created to demonstrate how to extend the solution to support custom limiters.

**JSON-style resource configuration**
ResourceRateLimiter supports (but does not require) JSON-style resource configuration to enable dynamic configuration of rules and resources through JSON hosted in an external source (API Project AppSettings.json file, JSON-based configuration data store, etc.)

A functional sample configuration file has also been provided here: [SampleResourceRateLimiterConfig.json](SampleResourceRateLimiterConfig.json)

* Note - A unit test was created in **ResourceRateLimiterTests** that utilizes this json file for demonstration purposes.

**Supported Rate Limiters**
ResourceRateLimiter supports all rate limiter rules provided by the [.NET NuGet RateLimiting package](https://www.nuget.org/packages/System.Threading.RateLimiting) package including:
* Concurrency
* Fixed Window
* Sliding Window
* Token Bucket

**RandomRateLimiter**
Additionally, to demonstrate adding custom rate limiting rules, the **RandomRateLimiter** class was created to demonstrate adding custom rate limiting algorithms to the configuration capabilities. This limiter is for example purposes only and should not be used in production.

Utilize rate limiting classes available in the official System.Threading.RateLimiting NuGet package.

### Solution Advantages

The solution utilizes classes and rules from the .Net NuGet library **System.Threading.RateLimiting.RateLimiter** rather than creating a custom rate limiter framework. This ensures the solution:
* Utilizes a Microsoft-maintained, core .NET NuGet package.
* Is .NET 6 compatible (this project's version) with support for future .NET versions.
* Adheres to industry best practices and standards.
* Increases likelihood of developer familiarity. 
* Reduces maintenance overhead by leveraging existing solutions.
* Reduces cognitive load for solution maintainers.
* Provides out of the box support for common rate limiting rules via built-in classes:
	* ConcurrencyLimiter: Limits the number of concurrent requests to a resources
	* FixedWindowRateLimiter: Limits the number of requests to a resource within a fixed window of time
	* SlidingWindowRateLimiter: Limits the number of requests to a resource within a sliding window of time
	* TokenBucketRateLimiter: Limits the number of requests to a resource within a fixed window of time, with a token bucket algorithm
* Provides extensibility through inheritance from base RateLimiter class.
* Implements composability through the use of the Chained/PartitionedRateLimiter class.

### Recommended Next Steps
Recommended next steps could include: 
* Refactor namespace for RateLimiter project to prevent conflicts with System.Threading.RateLimiting NuGet package.
* Implementing additional rate limiting rules as needed.
* Full testing of configuration and functionality for each supported rate limiter.
