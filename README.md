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

--

Notes and Requirements:

- Ability to support multiple rate limiting policies
- Rate limiting policy can be configurable (and extendable)
- Rate limiting policy can be combined?? Not sure how for now
	-	These policies can be combined by using the most restrictive rule, the least restrictive, or a commulative sum of both.
- There are rules that determine which rule applies to each client
- 


Initial intuition:

Create a tester class, this class will subscribe to the rate limiter class in order to limit calls made to it. (Can use existing test project)
Caller will call the session manager class to get a token (in lieu of an actual authenticator) and will store it in a global memory store held by the session manager class.
The test class will then make a call to a function in the API class using the token. The API class will call the session manager to verify the token.
The session manager class will verify the token then it should request from the rate limiting class the applicable rate limiting policies. This is essentially a subscribe to rate limiter call.
The rate limiting class will lookup (previously hardcoded for test purposes) the applicable rate limiting policies and store it in a memory store for subsequent lookup using the token id.
Each call in the test class will extract the token from the context and ask the session manager to validate token which in turn wiil ask the ratelimiter for a go or no go. if permitted then the flow continues, if not permitted it will return a 429 to the caller.

Possibly the rate limiter class should be an Interface or a base class and every rate limiting algorithm should be a concrete class. Note that the rate limiting functionality is coming through the session manager
by having the session manager apply the rate limiting through the rate limiter class(es) this way the implementation class, or test class in this case, doesn't know anything about rate limiting. Additionally the session manager doesn't know the details of the rate limiter.
This provides modularity and a separation of concerns between all three class types.

Cons of this approach:
1- We aren't limiting different calls differently, i.e. the limit applies to the token (and can be aggregated (by client id) in case of multiple users) but not granular to the endpoint level. So we cannot
allow a client to call function A 50 times/second and function B 100 times/second. If we want to provide the ability to rate limit different endpoints differently by client, then the session manager will need to provide the rate limiter the name of the function. 
However this approach is an antipattern because we are relying on the caller to tell the rate limiter what function is being called which opens the door for abuse. Ideally in that case we would have the session manager extract the name of the caller function by reflection if that's possible.
This is currently out of scope and will not be pursued.
2- This will only rate limit token based calls i.e. logged on users. For example this will not rate limit the SessionManager.Authenticate() call itself. Theoretically a malicious user can flood the authenticate call without a rate limit. This isn't intended to be a security or DOS attack prevention mechanism.
These types of attacks should be prevented at the network layer or the API Gateway.

So there are 4 types of classes in this exercise:

	1- Rate Limiter base class or interface. This class defines the basic structure for all rate limiters. Could implement the chaining or merging of policies and calculating limits.
	2- Rate Limiter concrete class(es). These classes have the concrete implementation of the Rate Limiter class above with different implementation for each type of limiter.
	3- Session manager class. This class will emulate an authentication and creates a token. This class looks up applicable policies for the user and creates the concrete rate limiting class(es) and passes them to the rate limiter manager
	4- A test class that will be rate limited. This class represents the endpoint that we're trying to limit. This could also be substituted by the unit tests.


Update: 08/13/2024

The final implementation consists of:

1- SessionManager Class. This class will emulate the authentication of users and will hold a live session key in a memory dictionary. This class will register new sessions with the RateLimiterBase class.
2- IRateLimiter Interface and ancillary objects. This is the interface upon which all rate limiter concrete classes are based
3- RateLimiterBase Class. This class will be called by SessionManager for each new session. It will lookup the users' session and client based ratelimiting rule definitions and will use a factory pattern to create these rules and save them in memory for lookp.
   These objects are client based keyed by the clientId, and session based keyed by the token.
4- Upon each request to validate the token at the SessionManager class, it will call the RateLimitingBase.Allow() method and pass it the user context. The RateLimiterBase will use the user context to look up the ratelimiting rules in memory.
	 It will then apply the client rules and session rules and determine whether the user is allowed to execute or not (Read comments in class for more info)
5- There is a series of tests in RateLimiter.Tests that attempt to test a few scenarios and edge cases.
6- Sample rate limiters provided are under the Rules folder: BypassRule, FixedWindowRule, and SlidingWindowRule. More rules can be added to limit by different algorithms and segmentation methods.