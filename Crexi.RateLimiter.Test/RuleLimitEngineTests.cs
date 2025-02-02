using Crexi.RateLimiter.Rule.Configuration.Sections;
using Crexi.RateLimiter.Rule.Enum;
using Crexi.RateLimiter.Rule.Execution;
using Crexi.RateLimiter.Rule.Model;
using Crexi.RateLimiter.Rule.ResourceAccess;
using Crexi.RateLimiter.Rule.Utility;
using Crexi.RateLimiter.Rule.Validation;
using Crexi.RateLimiter.Test.TestBase;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Crexi.RateLimiter.Test
{
    public class RuleLimitEngineTests : TestClassBase
    {
        #region AddUpdateRules

        [Fact]
        public void AddNewRules()
        {
            using var scope = TestHost.Services.CreateScope();
            SetupLogic(scope.ServiceProvider);
            var startupRules = scope.ServiceProvider
                .GetRequiredService<IOptions<RateLimitRulesConfiguration>>()
                .Value.StartupRules!.ToList();
            var mockAccess = SetupResourceAccess(scope.ServiceProvider, []);
            mockAccess.Setup(a => a.SetRules(It.IsAny<IEnumerable<RateLimitRule>>(), It.IsAny<CallData>()))
                .Returns((IEnumerable<RateLimitRule> rules, CallData callData) =>
                {
                    var callHash = CallDataComparer.GetHashCode(callData);
                    Assert.True(_expectedNewRules.TryGetValue(callHash, out var expectedRules));
                    Assert.Equal(expectedRules, rules, RuleComparer.Equals);
                    return mockAccess.Object;
                });
            var engine = scope.ServiceProvider.GetService<IRateLimitEngine>();
            engine!.AddUpdateRules(startupRules);
        }

        [Fact]
        public void AddUpdateExistingRules()
        {
            using var scope = TestHost.Services.CreateScope();
            SetupLogic(scope.ServiceProvider);
            var mockAccess = SetupResourceAccess(scope.ServiceProvider);
            mockAccess.Setup(a => a.SetRules(It.IsAny<IEnumerable<RateLimitRule>>(), It.IsAny<CallData>()))
                .Returns((IEnumerable<RateLimitRule> rules, CallData callData) =>
                {
                    var callHash = CallDataComparer.GetHashCode(callData);
                    Assert.True(_expectedUpdatedRules.TryGetValue(callHash, out var expectedRules));
                    var orderedExpected = expectedRules.OrderBy(RuleComparer.GetHashCode);
                    var orderedRules = rules!.OrderBy(RuleComparer.GetHashCode);
                    Assert.Equal(orderedExpected, orderedRules, RuleComparer.Equals);
                    return mockAccess.Object;
                });
            var engine = scope.ServiceProvider.GetService<IRateLimitEngine>();
            engine!.AddUpdateRules(_updateRules);
        }

        [Fact]
        public void SlidingWindowIsMaxOfRules()
        {
            using var scope = TestHost.Services.CreateScope();
            SetupLogic(scope.ServiceProvider);
            var mockAccess = SetupResourceAccess(scope.ServiceProvider, []);
            mockAccess.Setup(a => a.SetRules(It.IsAny<IEnumerable<RateLimitRule>>(), It.IsAny<CallData>()))
                .Returns(mockAccess.Object);
            mockAccess.Setup(a => a.SetExpirationWindow(It.IsAny<TimeSpan>(), It.IsAny<CallData>()))
                .Returns((TimeSpan ts, CallData cd) =>
                {
                    Assert.Equal(TimeSpan.FromMilliseconds(70), ts);
                    return mockAccess.Object;
                });
            var engine = scope.ServiceProvider.GetService<IRateLimitEngine>();
            engine!.AddUpdateRules(_slidingWindowRules);
        }

        #endregion AddUpdateRules

        #region Evaluate

        [Fact]
        public void EvaluateWillDeSpecifyUntilMatchIsFound()
        {
            ValueTuple<bool, int?> expectedResult = (true, null);
            using var scope = TestHost.Services.CreateScope();
            var mockLogic = SetupLogic(scope.ServiceProvider);
            mockLogic.Setup(l => l.EvaluateRule(It.IsAny<EvaluationType>(), It.IsAny<TimeSpan>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<CallHistory>()))
                .Returns((EvaluationType evaluationType, TimeSpan window, int? maxCallCount, int? overrideResponseCode,
                    CallHistory history) =>
                {
                    if (evaluationType != EvaluationType.TimespanSinceLastCall || 
                        window != TimeSpan.FromMilliseconds(5000) ||
                        maxCallCount.HasValue || overrideResponseCode.HasValue)
                        throw new Exception("Crud - we messed up");
                    return expectedResult;
                });
            var mockAccess = SetupResourceAccessForEvaluate(scope.ServiceProvider);
            mockAccess.Setup(a => a.AddCallAndGetHistory(It.IsAny<CallData>()))
                .Returns(new CallHistory());
            var engine = scope.ServiceProvider.GetService<IRateLimitEngine>();
            var result = engine!.Evaluate(_overSpecificBaseCallData);
            Assert.Equal(expectedResult, result);
        }

        #endregion Evaluate

        #region data

        private static readonly CallDataComparer CallDataComparer = new();
        private static readonly FullRateLimitRuleComparer RuleComparer = new();
        private const string TestPost = "HTTP: POST /test";
        private readonly CallData _baseCallData = new() { Resource = TestPost };
        private readonly CallData _overSpecificBaseCallData = new() { RegionId = 7, TierId = 2, ClientId = 3, Resource = TestPost };
        private readonly Dictionary<int, IList<RateLimitRule>> _expectedNewRules = new()
        {
            { CallDataComparer.GetHashCode(new CallData() { Resource = TestPost }), [ new RateLimitRule() { Resource = TestPost, Timespan = 5000, EvaluationType = EvaluationType.TimespanSinceLastCall }] },
            { CallDataComparer.GetHashCode(new CallData() { RegionId = 1, Resource = TestPost }), [ new RateLimitRule() { RegionId = 1, Resource = TestPost, Timespan = 5000, EvaluationType = EvaluationType.CallsDuringTimespan }] }
        };
        private readonly IList<RateLimitRule> _slidingWindowRules = 
        [
            new() { Resource = TestPost, Timespan = 50, EvaluationType = EvaluationType.TimespanSinceLastCall },
            new() { Resource = TestPost, Timespan = 60, EvaluationType = EvaluationType.CallsDuringTimespan },
            new() { Resource = TestPost, Timespan = 70, EvaluationType = EvaluationType.CallsDuringTimespan },
        ];

        private readonly IList<RateLimitRule> _updateRules = 
        [
            new() { Resource = TestPost, Timespan = 3000, EvaluationType = EvaluationType.TimespanSinceLastCall },
            new() { RegionId = 1, Resource = TestPost, Timespan = 77, EvaluationType = EvaluationType.TimespanSinceLastCall }
        ];
        private readonly Dictionary<int, IList<RateLimitRule>> _expectedUpdatedRules = new()
        {
            { CallDataComparer.GetHashCode(new CallData() { Resource = TestPost }), 
                [ 
                    new RateLimitRule() { Resource = TestPost, Timespan = 3000, EvaluationType = EvaluationType.TimespanSinceLastCall }
                ]
            },
            { CallDataComparer.GetHashCode(new CallData() { RegionId = 1, Resource = TestPost }), 
                [ 
                    new RateLimitRule() { RegionId = 1, Resource = TestPost, Timespan = 5000, EvaluationType = EvaluationType.CallsDuringTimespan },
                    new RateLimitRule() { RegionId = 1, Resource = TestPost, Timespan = 77, EvaluationType = EvaluationType.TimespanSinceLastCall },
                ]
            }
        };

        #endregion data

        #region setup

        private Mock<IRuleEvaluationLogic> SetupLogic(IServiceProvider serviceProvider)
        {
            var mockLogic = serviceProvider.GetService<Mock<IRuleEvaluationLogic>>();
            return mockLogic!;
        }

        private Mock<IRateLimitResourceAccess> SetupResourceAccess(IServiceProvider serviceProvider, IList<RateLimitRule>? getRules = null)
        {
            var mockResource = serviceProvider.GetService<Mock<IRateLimitResourceAccess>>();
            Assert.NotNull(mockResource);
            mockResource.Setup(r => r.GetRules(It.IsAny<CallData>()))
                .Returns((CallData cd) => getRules ?? _expectedNewRules[CallDataComparer.GetHashCode(cd)]);
            return mockResource;
        }

        private Mock<IRateLimitResourceAccess> SetupResourceAccessForEvaluate(IServiceProvider serviceProvider)
        {
            var mostDeSpecificHashCode = CallDataComparer.GetHashCode(_baseCallData);
            var mockResource = serviceProvider.GetService<Mock<IRateLimitResourceAccess>>();
            Assert.NotNull(mockResource);
            mockResource.Setup(r => r.GetRules(It.IsAny<CallData>()))
                .Returns((CallData cd) => CallDataComparer.Equals(cd, _baseCallData)
                    ? _expectedNewRules[mostDeSpecificHashCode]
                    : null);
            return mockResource;
        }

        #endregion setup

        #region configuration

        protected override void AddConfigurations(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json");
        }

        protected override IServiceCollection ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            return services
                    .Configure<RateLimiterConfiguration>(context.Configuration.GetSection(key: "RateLimiter"))
                    .Configure<RateLimitRulesConfiguration>(context.Configuration.GetSection(key: "RateLimiterRules"))
                    .AddScoped<IValidator<RateLimitRule>, RateLimitRuleValidator>()
                    .AddScopedMock<IRuleEvaluationLogic>()
                    .AddScopedMock<IRateLimitResourceAccess>()
                    .AddScoped<IRateLimitEngine, RateLimitEngine>()
                    .AddSingleton<TimeProvider, FakeTimeProvider>();
        }

        protected override IServiceCollection ConfigureServices(IServiceCollection services) =>
            services;


        protected override IServiceProvider ConfigureProviders(IServiceProvider serviceProvider) =>
            serviceProvider;

        #endregion configuration
    }
}