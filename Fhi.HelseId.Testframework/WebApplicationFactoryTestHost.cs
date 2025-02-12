using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.TestFramework
{

    /// <summary>
    /// Use if you have an existing entrypoint (top-level statements in Program.cs) file that you want to test
    /// </summary>
    public class WebApplicationFactoryTestHost<T> : WebApplicationFactory<T> where T : class
    {
        private IConfiguration? Configuration { get; }
        private Action<IServiceCollection> ServiceCollection { get; }

        public WebApplicationFactoryTestHost(IConfiguration configuration, Action<IServiceCollection> services)
        {
            Configuration = configuration;
            ServiceCollection = services;
        }

        public WebApplicationFactoryTestHost(Action<IServiceCollection> services)
        {
            ServiceCollection = services;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (Configuration is not null)
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddConfiguration(Configuration);
                });
            }

            builder.ConfigureTestServices(ServiceCollection.Invoke);
        }
    }
}
