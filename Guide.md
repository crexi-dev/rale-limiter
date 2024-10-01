# Overview

## How It Works
1. Rate limiter rules implement IRule. Rule instances have a unique name and contain specific configuration.
2. Rate limiter supporting services and rules are registered in the DI container.
3. Rules are connected to endpoints by attach one or more `RateLimiterEndpointMetadata` to endpoints. 
    - The metadata lists the desired rules (by name) for that endpoint.
4. Rule to resource mapping are populated on startup and cached in the RuleService.
5. A RateLimiterMiddleware executes the rules on every request (via the RuleService). 
    - It also triggers recording of request counts. It writes to a Channel which is then read by the request count store.

## Setup

1. Register the rate limiter services and any desired rules in the DI container e.g.
    - Rule intances should be registered as singletons. 
    - If you're using the default in memory request count store, please also call the `AddMemoryCache` method.
    
 ```c#
    static void AddRateLimiterRules(IServiceCollection services)
    {
        var fiveRequestsPerMinuteRule = (IServiceProvider sp) => new CountPerTimespanRule(
                RateLimiterRuleNames.FiveRequestsPerMinute,
                sp.GetRequiredService<ITimeProvider>(),
                sp.GetRequiredService<IRequestCountStore>(),
                new CountPerTimespanRuleOptions(5, TimeSpan.FromMinutes(1)));

        var oneMinuteSinceLastRequestRule = (IServiceProvider sp) => new TimespanSinceLastCallRule(
                RateLimiterRuleNames.OneMinuteSSinceLastRequest,
                sp.GetRequiredService<ITimeProvider>(),
                sp.GetRequiredService<IRequestCountStore>(),
                new TimespanSinceLastCallRuleOptions(TimeSpan.FromMinutes(1)));

        services.AddRateLimiter();
        services.AddMemoryCache(); // only if using the default in memory store
        services.AddSingleton<IRule>(fiveRequestsPerMinuteRule);
        services.AddSingleton<IRule>(oneMinuteSinceLastRequestRule);
        services.AddSingleton<IRule>((sp) =>
        {
            return new CountrySpecificRule(
                RateLimiterRuleNames.CountrySpecific,
                new Dictionary<string, IEnumerable<IRule>>()
                {
                    ["US"] = new IRule[] { fiveRequestsPerMinuteRule(sp) },
                    ["FR"] = new IRule[] { oneMinuteSinceLastRequestRule(sp) }
                });
        });
    }
 ``` 

2. Attach rules to endpoints by attaching rule options as endpoint metadata e.g.
    - This is an example of attaching rules endpoint by endpoint. 
    - You can also attach rules in bulk by modifying the [application model](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/application-model?view=aspnetcore-8.0).

```c#
    app.MapGet("/test", () => "test")
        .WithName("GetTest")
        .WithMetadata(new RuleEndpointMetadata(new string[] { RateLimiterRuleNames.FiveRequestsPerMinute }));

    app.MapPost("/test", () => "test")
        .WithName("PostTest");

    app.MapGet("/test2", () => "test2")
        .WithName("GetTest2")
        .WithMetadata(new RuleEndpointMetadata(new string[] { RateLimiterRuleNames.OneMinuteSSinceLastRequest }));

    app.MapGet("/test3", () => "test3")
        .WithName("GetTest3")
        .WithMetadata(new RuleEndpointMetadata(new string[] { RateLimiterRuleNames.CountrySpecific }));
 ```


 3. Add the RateLimiter middleware to the pipeline (startup). Make sure to add the rate limiter middleware AFTER 
the Authentication and Routing middleware. You can add the RateLimiterMiddleware by calling the UseRateLimiter extension method e.g. 
    
 ```c#
    app.UseRateLimiter();
```

4. Populate the rules on startup by calling the PopulateRateLimiterRules on the endpoint builder e.g.

```c#
    app.PopulateRateLimiterRules(app.Services.GetServices<IRule>(), app.Services.GetRequiredService<IRuleService>());
```

 That's it!

# Extensibility

## New Rules

To create new rules, implement the IRule interface. Then register the rule in DI and attach it to desired endpoints.

## New Middleware

The default middleware is responsible for:
    
    1. Invoking the rule service for the current request.
    2. Skipping the remainder of the pipeline if rate limits have been exceeded.
    3. Invoking the counter to track the current request if rate limits have not been exceeded.

You can register your own middleware in the pipeline (instead of the default one) to do the same tasks.


## New Store

The default store is an in-memory store that stores request counts in the memory cache.

To use a different store (e.g. redis, relational db), register the new implementation of `IRequestCountStore`in the DI container
as a singleton. The new store should accept a `ChannelReader<NewRequest> reader` parameter in its constructor so that it 
can consume the channel e.g.

```c#
    public InMemoryRequestCountStore(
        IMemoryCache cache,
        ChannelReader<NewRequest> reader)
    {
        this.cache = cache;
        this.reader = reader;

        ConsumeReader();
    }
```

## Replacing the Channel

The default channel is a bounded channel with a capacity of 1,000,000 items. Using a bounded channel provides some
backpressure when messages are not being consumed as fast as they are produced. The channel is configured
to wait once the max capacity is reached.

To use different options, create your own channel and register its writer and reader as singletons in the DI container (AFTER the rate limiter services have been added) e.g.

```c#
    var channel = Channel.CreateBounded<NewRequest>(new BoundedChannelOptions(1000000)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleWriter = false,
        SingleReader = true,
    });

    services.AddSingleton(channel.Reader);
    services.AddSingleton(channel.Writer);
```

# FAQ

## Why Does This Use [Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)?

Channels provide a few benefits in a scenario like rate limiting where there is high concurrency, multiple writers and 
usually a single data store. Those benefits include:

- Decouple execution of rate limiting rules from request counting.
- Encapasulates thread safety code  necessary for concurrency scenarios i.e. lock free code
- Provides backpressure if a sudden spike in traffic surpasses the processing rate of the request counter
- Allows for easier batching and processing of request counts

The benefits are somewhat hidden with an in-memory store (little to no IO) but show up significantly once a real data store is used.

There are tradeoffs. In exchange for the benefits above, there are some downsides:

- It can be slightly harder to test the store since recording a request happens asynchronously (tasks). See
the InMemoryRequestCounterStoreTests handles this.
- Recording of requests might not be instantaneous if consuming the channel is slow.

## What is the ITimeProvider

Testing rate limiting requires controlling the passage of time. The ITimeProvider is a wrapper around DateTimeOffset.UtcNow. The time provider
is mocked in tests to allow for fine-grained control over the passage of time.

## Caveats

The library is setup for granular, short term rate limiting (<24 hours as opposed to days, weeks). The default store (in memory) cleans up inactive users
after 24 hours. 
