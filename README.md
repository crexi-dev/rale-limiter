
# RateLimiter Library

The RateLimiter is a reusable class library that can be cloned into any project and referenced to utilize rate-limiting functionalities.

# Project Setup
#### 1. Clone the RateLimiter Project:
Clone the RateLimiter project into your solution.

#### 2. Add Project Reference:
Add the RateLimiter project as a reference in your project.

#### 3. Configure Rate Limiter Settings:
Copy the ratelimitersettings.json file into your project, or create a new JSON file using the following format:


```bash
  
{
  "RateLimiter": {
    "RuleA": {
      "RequestsPerTimespan": 5,
      "TimespanSeconds": "00:00:20"
    },
    "RuleB": {
      "MinTimespanBetweenCallsSeconds": "00:00:30"
    }
  }
}
```


#### 4. Register Dependencies:
Register the RateLimiter dependencies in your IoC container as shown below:


```bash
  builder.Services.AddRateLimiter(builder.Configuration);
  ```
#### 5. Inject and Use Rate Limit Rules:
Inject the registered factory service into your target class and use the rate limit rules as follows:
```bash
private readonly Func<RateLimitRules, IRateLimitRule> _func;

public Resource(Func<RateLimitRules, IRateLimitRule> func)
{
    _func = func;
}

var ruleAService = _func(RateLimitRules.RuleA);

RateLimitRuleRequestDto userInfo = new()
{
    UserId = 1,
    UserLocale = "US"
};

bool isAllowed = ruleAService.IsRequestAllowed(userInfo);
 ```
#### Note: 
To access different services, use the following enum service key indicators:
```bash
public enum RateLimitRules
{
    RuleA,
    RuleB,
    RuleC
}
 ```

### Usage in APIs
For API resources, the rate limit logic can be applied via direct injection, an attribute on the resource, or middleware in the pipeline. A sample attribute implementation is shown below:
```bash
[RateLimitAttribute(new RateLimitRules[] { RateLimitRules.RuleA })]
[RateLimitAttribute(new RateLimitRules[] { RateLimitRules.RuleA, RateLimitRules.RuleB, RateLimitRules.RuleC })]
 ```

#### Note: The attribute constructor accepts the rules enum to apply the specified rules.






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
