using System.Net;
using System.Security.Claims;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.TestFramework.Extensions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.TestFramework.Samples.HostBuilders
{
    internal class TestWebApplicationFactorySamples
    {
        [Test]
        public async Task UsingWebTestWebApplicationFactory_ConfigureServices()
        {
            var inMemoryConfig = CreateConfig();

            var appFactory = new WebApplicationFactoryTestHost<Program>(
                inMemoryConfig,
                services =>
                {
                    services.AddFakeTestAuthenticationScheme(
                     [
                         new Claim(IdentityClaims.Name, "Line danser")
                     ]);
                    services.AddHelseIdWebAuthentication(inMemoryConfig).Build();
                });

            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

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
