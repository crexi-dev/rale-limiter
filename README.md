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

The solution implements Limiters that provide Leases to requesting Resources. Limiters are configured through a common **LimiterConfig** file that contains all the configuration properties that can be applied for all limiters. It is the responsibilty of each limiter to ensure the proper configuration is set in this class for the given limiter type. 

Leases are produced when a resource attempts to utilize a limiter. The lease may be acquired or denied based on the current state of the limiter. Leases are configured using the **LeaseConfig** class. For example, the **TokenLimiter** class uses **LeaseConfig** to enable requesting a lease with a variable amount of tokens.

Relinquishing a lease releases the limiter according to the limiter implementation.

**Linked Limiters**

It is possible to compose limiter rules using the **LinkedLimiter** class. 

- Any combination of limiters can be created, including chaining multiple limiters of the same type together.
	- Example: Rule A + Rule B + Rule C + Rule A  is a supported use case for a linked limiter.
- When acquiring a lease, all linked limiters must succeed, or the lease acquisition fails as a unit.
- Nesting of linked limiters is not currently supported, but could be with an enhancement to the linked limiter.
	- Example: Rule A + (Rule B + Rule C) + Rule D is not currently supported

**Example Limiters**

Two example limiters have been created: TokenLimiter and FixedWindowLimiter. 

**TokenLimiter** 

- Sets a maximum number of tokens that can be utilized at one time.
- Provids the ability to configure the number of tokens each resource acquires for a given lease.
- As leases are acquired, the number of tokens utilized is incremented.
- As leases are released, the number of tokens utilized is decremented.

**FixedWindowLimiter** 

- A simple limiter which takes the window duration in seconds and the maximum number of resources that can utilize the limiter for the given window
- The number of uses increases for each successful lease and is only reset when the window's elapsed time causes a token refresh.

**JSON-style resource configuration**

ResourceRateLimiter supports (but does not require) JSON-style resource configuration to enable dynamic configuration of rules and resources through JSON hosted in an external source (API Project AppSettings.json file, JSON-based configuration data store, etc.)

A functional sample configuration file has also been provided here: [SampleResourceRateLimiterConfig.json](SampleResourceRateLimiterConfig.json)

* Note - A unit test was created in **ResourceRateLimiter.Tests** that utilizes this json file for demonstration purposes.

### Potential Future Enhancements:

1. Support nested linked limiters to support additional combinations of limiters.
	Example: Rule A + Rule B + (Rules A + B) + ((Rules A + B) + (Rules B + C)).
2. Support for asynchronous calls.
3. Support for resource queuing.
