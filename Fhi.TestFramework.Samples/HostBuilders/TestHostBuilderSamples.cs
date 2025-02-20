using System.Net;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.TestFramework.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Fhi.TestFramework.Samples.HostBuilders
{
    /// <summary>
    /// Using HostBuilder with TestServer
    /// </summary>
    public class TestServerSamples
    {
        [Test]
        public async Task UsingHostBuilder()
        {
            var inMemoryConfig = CreateConfig();

            var builder = new HostBuilder().ConfigureWebHost(builder =>
            {
                var webHostBuilder = builder.UseTestServer();
                webHostBuilder.ConfigureServices(services =>
                {
                    services.AddHelseIdWebAuthentication(inMemoryConfig).Build();
                });
                webHostBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(builder =>
                    {
                        builder.MapGet("/api/test-endpoint", () => "Hello world!");
                    });
                });
                webHostBuilder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddConfiguration(inMemoryConfig);
                });
            });
            var host = builder.Build();
            host.Start();

            var sever = host.GetTestServer();

            var client = sever.CreateClient();
            var response = await client.GetAsync("api/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        private static IConfigurationRoot CreateConfig()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                { "HelseIdWebKonfigurasjon:AuthUse", "false" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" }
            };
            var inMemoryConfig = appsettingsConfig.BuildInMemoryConfiguration();
            return inMemoryConfig;
        }
    }
}
