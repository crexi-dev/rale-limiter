using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crexi.RateLimiter.Test.TestBase
{
    public abstract class TestClassBase
    {
        protected readonly IHost TestHost;
        private readonly Task _hostTask;

        protected IServiceProvider ServiceProvider => TestHost.Services;

        protected TestClassBase()
        {
            TestHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(AddConfigurations)
                .ConfigureLogging(AddLogging)
                .ConfigureServices((context, services) =>
                {
                    InternalConfigureServices(services);
                    ConfigureServices(context, services);
                    ConfigureServices(services);
                })
                .Build();
            InternalConfigureProviders(TestHost.Services);
            ConfigureProviders(TestHost.Services);
            _hostTask = TestHost.RunAsync();
        }

        public static readonly ILoggerFactory ConsoleLoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        protected T GetService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

        protected virtual void AddLogging(ILoggingBuilder builder) => builder.AddDebug().AddConsole();

        protected virtual void AddConfigurations(IConfigurationBuilder builder) { }

        protected virtual IServiceCollection InternalConfigureServices(IServiceCollection services) => services;

        protected abstract IServiceCollection ConfigureServices(IServiceCollection services);

        protected virtual IServiceCollection ConfigureServices(HostBuilderContext context, IServiceCollection services) => services;

        protected abstract IServiceProvider ConfigureProviders(IServiceProvider serviceProvider);

        protected virtual IServiceProvider InternalConfigureProviders(IServiceProvider serviceProvider) => serviceProvider;

        ~TestClassBase()
        {
            _hostTask.Dispose();
        }
    }
}
