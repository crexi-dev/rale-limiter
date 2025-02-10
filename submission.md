# RateLimiter
A class library for providing configurable and extensible rate limiting for web applications.
***
## Approach
lorem ipsum
***
## Decisions/Assumptions
Per the instructions, most of the time was spent around designing the rate limiting framework itself with much less concern about the implementation details for each of the four algorithms.

While unit tests are provided for each, no time was spent running benchmarks in attempts to tweak performance and minimize memory usage.
***
## Registration, Configuration & Usage
lorem ipsum
***
### Service Registration
Registration of _RateLimiter's_ required services is provided via a fluent api.

Example:
```
builder.Services.AddRateLimiting()
    .WithConfiguration<RateLimiterConfiguration>(builder.Configuration.GetSection("RateLimiter"));
```
***
### Configuration
_RateLimiter_ can be configured via a standard appSettings.json section (or other configuration provider, i.e. Azure App Config) or via use of a fluent api.

#### AppSettings.json Configuration
Configuration spec:
<a name="json-config-anchor-point"></a>
```
"RateLimiter": {
  "DefaultAlgorithm": "FixedWindow|LeakyBucket|SlidingWindow|TokenBucket",
  "DefaultMaxRequests": <int>,
  "DefaultTimespanMilliseconds": <int>,
  "Rules": [
	{
	  "Name": "MyDistinctRuleName",
	  "Type": "RequestPerTimespan|TimespanElapsed",
	  "Discriminator": "Custom|GeoLocation|IpAddress|IpSubnet|QueryString|RequestHeader",
	  "DiscriminatorMatch": <string?>,
	  "DiscriminatorCustomType": <string?>,
	  "MaxRequests": <int?>,
	  "TimespanMilliseconds": <int?>,
	  "Algorithm": "Default|FixedWindow|LeakyBucket|SlidingWindow|TokenBucket"
	}
  ]
}
```
#### FluentApi Configuration
~~TBD~~ (will not be implemented at this time; please use json-based configuration)

***
### Usage in Controller-Based Applications
Registration of a rate limiting rule (or multiple rules) requires an attribute with a single parameter - the distinct name of the rule configured within the RateLimiter.Rules section.

Example usage:

```
[RateLimitedResource(RuleName="MyFirstDistinctRuleName")]
[RateLimitedResource(RuleName="MySecondDistinctRuleName")]
[HttpGet(Name="GetWeatherForecast")]
public IEnumerable<WeatherForecast> Get() {
  // implementation
}
```
***
### Usage in MinimalApi-Based Application
Registration of a rate limiting rule (or multiple rules) requires usage of the FluentApi with a single parameter - the distinct name of the rule configured within the RateLimiter.Rules section.

Example usage:
```
app.MapGet("/weatherforecast", () =>
{
   // implementation
})
.WithName("GetWeatherForecast")
.WithRateLimitingRule("MyFirstDistinctRuleName")
.WithRateLimitingRule("MySecondDistinctRuleName");
```
***
## Internal Class Hierarchy
lorem ipsum
***
## Configurability
lorem ipsum
***
## Extensibility
Consumers can add their own custom discriminators for more complex scenarios.  The process of doing so consists of 3 parts:

1. Provide a class that implements _IProvideADiscriminator_.
2. Create a rule in your [json-based configuration](#json-config-anchor-point) that specifies that class name in the _DiscriminatorCustomType_ property on a _Rules_ entry.
3. Modify the service registration to include your custom discriminator as shown below

```
builder.Services.AddRateLimiting()
    .WithCustomDiscriminator<MyCustomDiscriminator>()
    .WithConfiguration<RateLimiterConfiguration>(builder.Configuration.GetSection("RateLimiter"));
```

Multiple custom discriminators can be added provided they each have a unique name.  A run-time exception will be thrown immediately upon application start in the case of a duplicated name.