using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.TestFramework.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Fhi.TestFramework.Samples.HostBuilders
{

    internal class TestWebApplicationBuildersamples
    {
        [Test]
        public async Task UsingWebApplicationBuilderTestHost()
        {
            var appSettings = CreateConfig();

            var builder = WebApplicationBuilderTestHost
                .CreateWebHostBuilder()
                .WithConfiguration(appSettings)
                .WithServices(services =>
                {
                    services.AddHelseIdWebAuthentication(appSettings).Build();
                });

            var app = builder.BuildApp(app =>
            {
                app.UseRouting();
                app.MapGet("/api/test-endpoint", () => "Hello world!");
            });
            app.Start();

            using var client = app.GetTestClient();
            var response = await client.GetAsync("api/test-endpoint");
        }


        private static IConfigurationRoot CreateConfig()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                { "HelseIdWebKonfigurasjon:AuthUse", "false" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" }
            };
            var config = appsettingsConfig.BuildInMemoryConfiguration();
            return config;
        }
    }
}
