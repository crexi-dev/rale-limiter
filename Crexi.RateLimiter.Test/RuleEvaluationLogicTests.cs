using Crexi.RateLimiter.Rule.Configuration;
using Crexi.RateLimiter.Rule.Enum;
using Crexi.RateLimiter.Rule.Execution;
using Crexi.RateLimiter.Rule.Model;
using Crexi.RateLimiter.Test.TestBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Crexi.RateLimiter.Test
{
    public class RuleEvaluationLogicTests: TestClassBase
    {
        #region CallsDuringTimespan

        [Fact]
        public void CallsDuringTimespanUnderMaxCalls()
        {
            var logic = GetService<IRuleEvaluationLogic>();
            var timeProvider = GetService<TimeProvider>();
            var (success, responseCode) = logic.EvaluateRule(EvaluationType.CallsDuringTimespan, TimeSpan.FromMilliseconds(30), 6, null,
                CallHistoryFiveCallsInLast30Ms(timeProvider));
            Assert.True(success);
            Assert.Null(responseCode);
        }

        [Fact]
        public void CallsDuringTimespanEqualMaxCalls()
        {
            var logic = GetService<IRuleEvaluationLogic>();
            var timeProvider = GetService<TimeProvider>();
            var (success, responseCode) = logic.EvaluateRule(EvaluationType.CallsDuringTimespan, TimeSpan.FromMilliseconds(30), 5, OverrideResponseCode,
                CallHistoryFiveCallsInLast30Ms(timeProvider));
            Assert.False(success);
            Assert.Equal(OverrideResponseCode, responseCode);
        }

        [Fact]
        public void CallsDuringTimespanOverMaxCalls()
        {
            var logic = GetService<IRuleEvaluationLogic>();
            var timeProvider = GetService<TimeProvider>();
            var (success, responseCode) = logic.EvaluateRule(EvaluationType.CallsDuringTimespan, TimeSpan.FromMilliseconds(30), 4, OverrideResponseCode,
                CallHistoryFiveCallsInLast30Ms(timeProvider));
            Assert.False(success);
            Assert.Equal(OverrideResponseCode, responseCode);
        }

        #endregion CallsDuringTimespan

        #region TimespanSinceLastCall

        [Fact]
        public void TimespanSinceLastCallUnderWindow()
        {
            var logic = GetService<IRuleEvaluationLogic>();
            var timeProvider = GetService<TimeProvider>();
            var (success, responseCode) = logic.EvaluateRule(EvaluationType.TimespanSinceLastCall, TimeSpan.FromMilliseconds(31), null, OverrideResponseCode,
                CallHistoryLastCall30MsAgo(timeProvider));
            Assert.False(success);
            Assert.Equal(OverrideResponseCode, responseCode);
        }

        [Fact]
        public void TimespanSinceLastCallInWindow()
        {
            var logic = GetService<IRuleEvaluationLogic>();
            var timeProvider = GetService<TimeProvider>();
            var (success, responseCode) = logic.EvaluateRule(EvaluationType.TimespanSinceLastCall, TimeSpan.FromMilliseconds(30), null, OverrideResponseCode,
                CallHistoryLastCall30MsAgo(timeProvider));
            Assert.False(success);
            Assert.Equal(OverrideResponseCode, responseCode);
        }

        [Fact]
        public void TimespanSinceLastCallOverWindow()
        {
            var logic = GetService<IRuleEvaluationLogic>();
            var timeProvider = GetService<TimeProvider>();
            var (success, responseCode) = logic.EvaluateRule(EvaluationType.TimespanSinceLastCall, TimeSpan.FromMilliseconds(29), null, null,
                CallHistoryLastCall30MsAgo(timeProvider));
            Assert.True(success);
            Assert.Null(responseCode);
        }

        #endregion TimespanSinceLastCall

        #region WeAreTeasing

        [Fact]
        public void WeAreTeasing()
        {
            var logic = GetService<IRuleEvaluationLogic>();
            var timeProvider = GetService<TimeProvider>();
            var (success, responseCode) = logic.EvaluateRule(EvaluationType.WeAreTeasing, TimeSpan.FromMilliseconds(31), null, OverrideResponseCode,
                CallHistoryLastCall30MsAgo(timeProvider));
            Assert.False(success);
            Assert.Equal(406, responseCode);
        }

        #endregion WeAreTeasing

        #region WeArePseudoConfusing

        [Fact]
        public void WeArePseudoConfusing()
        {
            var logic = GetService<IRuleEvaluationLogic>();
            var timeProvider = GetService<TimeProvider>();
            var (success, responseCode) = logic.EvaluateRule(EvaluationType.WeAreTeasing, TimeSpan.FromMilliseconds(31), null, OverrideResponseCode,
                CallHistoryLastCall30MsAgo(timeProvider));
            Assert.Equal(406, responseCode);
        }

        #endregion WeArePseudoConfusing

        #region data

        private const int OverrideResponseCode = 1;

        private static CallHistory CallHistoryFiveCallsInLast30Ms(TimeProvider timeProvider) => new()
        {
            Calls = [ timeProvider.GetUtcNow().DateTime, timeProvider.GetUtcNow().DateTime, timeProvider.GetUtcNow().DateTime, timeProvider.GetUtcNow().DateTime, timeProvider.GetUtcNow().DateTime, timeProvider.GetUtcNow().DateTime.Subtract(TimeSpan.FromMilliseconds(31)),  ]
        };

        private static CallHistory CallHistoryLastCall30MsAgo(TimeProvider timeProvider) => new()
        {
            LastCall = timeProvider.GetUtcNow().DateTime.Subtract(TimeSpan.FromMilliseconds(30))
        };

        #endregion data


        #region configuration

        protected override void AddConfigurations(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json");
        }

        protected override IServiceCollection ConfigureServices(HostBuilderContext context, IServiceCollection services) =>
            services
                .ConfigureRateLimiterRules(context.Configuration)
                .AddSingleton<TimeProvider, FakeTimeProvider>();

        protected override IServiceCollection ConfigureServices(IServiceCollection services) =>
            services;


        protected override IServiceProvider ConfigureProviders(IServiceProvider serviceProvider) =>
            serviceProvider;

        #endregion configuration
    }
}