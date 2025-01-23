**Rate Limiter Take Home Implementation**

Two rate limit policies are available in this iteration:
* Sliding time window - For each unique client ID, this policy limits the number of calls made within the last time window (TimeSpan) 
* Concurrent requests - For each unique client ID, this policy limits the active number of requests. When requests complete, middleware needs to make a call to mark a request as complete

**Filter groups**

Filter groups allow policies to be either applied or excluded depending on properties in the client request. Filter groups can contain multiple filters and combinations
* Region -  https://en.wikipedia.org/wiki/ISO_3166-2 - ISO-3166-2 format with 4 letter codes for country and state/province. Examples for California and British Columbia: US-CA, CA-BC
* Subscription level - FREE, PREMIER, PARTNER

**Example Use Cases for Filter Groups**
* California users can have a different override limits by subscription level. Free tier users can have a lowere limit that premium
* Overlapping filter groups are not fully supported. The first set of group filters that match will be the the one that applies. For example, if there is an override rule for all PREMIUM users and another override for California PREMIUM users, the override that is defined first will be applied 

**Key components**

The logic can be divided into the following components
* Rate limit engine
* Client request tracker
* Rate policy evaluators

Tracking of client requests is separate from rate policy evaluation. The rate limit engine ties all the components together, acting as the main entry point for the library

**Limitations/Future implementation**

* Client request tracking is in-memory and cannot scale to high level requests
* Tied to the in-memory limitation, request history is not capped. A background process to clear old requests or inline pruning would solve this
* Token based or fixed window rate limit policies
* Wildcards for filters could be a good shortcut
* Configuration/Dependency Injection for policy configuration

___

**PROBLEM STATEMENT**

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
