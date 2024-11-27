using RateLimiterWeb.RateLimiting;
using RateLimiter.Components.RuleService.Rules.RuleNRequestPerTimerange;
using RateLimiter.Components.RuleService.Rules.RuleAllow1RequestForMatchingConfiguration;
using RateLimiter.Components.RulesService.Rules.DummyRule;
using RateLimiter.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.





// register filter
builder.Services.AddControllers((options) => options.Filters.Add<RateLimitingFilter>());

builder
    .Services
    // add infrastructure 
    .AddRateLimiter()
    // rules in the global group will run for every call 
    .AddRateLimiterGlobalGroup(
        (options) =>
        {
            options.RuleName = nameof(DummyRule);
        }
    )
    // rules set in a group will only run when requested (using attribute)
    .AddRateLimiterGroup(
        "group1", 
        (options) => 
        {
            options.RuleName = nameof(RuleNRequestPerTimerange);
            options.Configuration = new RateLimitingRuleConfiguration()
            {
                Timerange = TimeSpan.FromSeconds(3),
                NumberOfRequests = 3
            };
        },
        // this is just to show that a rule can be reused in multiple groups
        (options) =>
        {
            options.RuleName = nameof(DummyRule);
        }
    )
    .AddRateLimiterGroup(
        "group2",
        (options) =>
        {
            options.RuleName = nameof(RuleAllow1RequestForMatchingConfiguration);
            options.Configuration = new RateLimitingRuleConfiguration()
            {
                // these elements allow the rule to run for specific scenarios
                Controller = "WeatherForecast",
                Action = "Get",
                Country = "US",
                Parameters = new List<string>() { "id" }
            };
        }
    )
    ;






var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
