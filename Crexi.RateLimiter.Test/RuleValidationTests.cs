using Crexi.RateLimiter.Rule.Configuration;
using Crexi.RateLimiter.Rule.Enum;
using Crexi.RateLimiter.Rule.Model;
using Crexi.RateLimiter.Test.ShallowMocks;
using Crexi.RateLimiter.Test.TestBase;
using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Crexi.RateLimiter.Test
{
    public class RuleValidationTests: TestClassBase
    {
        private readonly IEnumerable<EndpointDataSource> _endpointDataSources = [ new MockEndpointDataSource() ];

        #region Resource

        [Fact]
        public void ResourceCannotBeEmpty()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = string.Empty,
                Timespan = 100,
                EvaluationType = EvaluationType.TimespanSinceLastCall,
            };
            var result = validator.Validate(rule);
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Exists(e => e.ErrorMessage == "Must provide a target resource."));
        }

        [Fact]
        public void ResourceMustBeRegistered()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: GET /another/service/endpoint",
                Timespan = 100,
                EvaluationType = EvaluationType.TimespanSinceLastCall,
            };
            var result = validator.Validate(rule);
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Exists(e => e.ErrorMessage == "Not a valid resource for this service."));
        }

        [Fact]
        public void ValidResourcePasses()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 100,
                EvaluationType = EvaluationType.TimespanSinceLastCall,
            };
            var result = validator.Validate(rule);
            Assert.True(result.IsValid);
        }

        #endregion Resource

        #region TimeSpan

        [Fact]
        public void TimeSpanMustBeLessThanConfiguredLimit()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 300001,
                EvaluationType = EvaluationType.CallsDuringTimespan,
            };
            var result = validator.Validate(rule);
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Exists(e => e.ErrorMessage == "Requested timespan exceeds the max configured limit of 5 minutes."));
        }

        [Fact]
        public void TimeSpanIsLessThanConfiguredLimit()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 299999,
                EvaluationType = EvaluationType.TimespanSinceLastCall,
            };
            var result = validator.Validate(rule);
            Assert.True(result.IsValid);
        }

        #endregion TimeSpan

        #region TimeSpan

        [Fact]
        public void MaxCallCountNotRequiredForTimespanSinceLastCall()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 1,
                EvaluationType = EvaluationType.TimespanSinceLastCall,
            };
            var result = validator.Validate(rule);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void MaxCallCountRequiredForCallsDuringTimespan()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 300001,
                EvaluationType = EvaluationType.CallsDuringTimespan,
            };
            var result = validator.Validate(rule);
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Exists(e => e.ErrorMessage == "Must provide max calls when evaluating CallsDuringTimespan."));
        }

        [Fact]
        public void MaxCallCountForCallsDuringTimespan()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 1,
                EvaluationType = EvaluationType.CallsDuringTimespan,
                MaxCallCount = 5,
            };
            var result = validator.Validate(rule);
            Assert.True(result.IsValid);
        }

        #endregion TimeSpan

        #region EvaluationType

        [Fact]
        public void EvaluationTypeMustExist()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 100,
                EvaluationType = default,
            };
            var result = validator.Validate(rule);
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Exists(e => e.ErrorMessage == "Must provide a valid evaluation type."));
        }

        #endregion EvaluationType

        #region EffectiveWindow

        [Fact]
        public void EffectiveWindowBothValuesMustBeProvided_Start()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 100,
                EvaluationType = EvaluationType.TimespanSinceLastCall,
                EffectiveWindowStartUtc = new TimeOnly(1, 19)
            };
            var result = validator.Validate(rule);
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Exists(e => e.ErrorMessage == "EffectiveWindowEndUtc must be provided when EffectiveWindowStartUtc is provided."));
        }

        [Fact]
        public void EffectiveWindowBothValuesMustBeProvided_End()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 100,
                EvaluationType = EvaluationType.TimespanSinceLastCall,
                EffectiveWindowEndUtc = new TimeOnly(1, 19)
            };
            var result = validator.Validate(rule);
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Exists(e => e.ErrorMessage == "EffectiveWindowStartUtc must be provided when EffectiveWindowEndUtc is provided."));
        }

        [Fact]
        public void EffectiveWindowStartMustBeBeforeEnd()
        {
            var validator = GetService<IValidator<RateLimitRule>>();
            var rule = new RateLimitRule()
            {
                Resource = "HTTP: POST /test",
                Timespan = 100,
                EvaluationType = default,
                EffectiveWindowStartUtc = new TimeOnly(23, 19),
                EffectiveWindowEndUtc = new TimeOnly(1, 19)
            };
            var result = validator.Validate(rule);
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Exists(e => e.ErrorMessage == "EffectiveWindowStartUtc must be before EffectiveWindowEndUtc."));
        }

        #endregion EffectiveWindow


        #region configuration

        protected override void AddConfigurations(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json");
        }

        protected override IServiceCollection ConfigureServices(HostBuilderContext context, IServiceCollection services) =>
            services
                .ConfigureRateLimiterRules(context.Configuration)
                .AddSingleton<FakeTimeProvider>()
                .AddScoped(_ => _endpointDataSources);

        protected override IServiceCollection ConfigureServices(IServiceCollection services) =>
            services;


        protected override IServiceProvider ConfigureProviders(IServiceProvider serviceProvider) =>
            serviceProvider;

        #endregion configuration
    }
}