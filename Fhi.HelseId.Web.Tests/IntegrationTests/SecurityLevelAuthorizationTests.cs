using System.Net;
using Fhi.HelseId.Web.Common;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.IntegrationTests.Setup;
using Fhi.TestFramework;
using Fhi.TestFramework.Extensions;
using Fhi.TestFramework.NHNTTT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Fhi.HelseId.Web.IntegrationTests
{
    internal class SecurityLevelAuthorizationTests
    {
        /// <summary>
        /// Required security level is missing from token
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task SecurityLevelConfigured_AuthenticatedUserCallingAPIWithMissingSecurityLevelInToken_Return403()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create.Default().WithSecurityLevel(["3"]);
            var (accessToken, idToken) = await CreateAccessAndIdToken(config.ClientId, config.AllScopes.ToList(), securityLevel: null);

            var appSettings = config.CreateConfigurationRoot();
            var app = CreateApplicationBuilderWithConfiguration(appSettings)
               .WithServices(ConfigureHelseIdAuthenticationAndFakeAuthentication(accessToken, idToken, appSettings))
               .BuildApp(UseEnpointAuthenticationAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        /// <summary>
        /// Security level in token is higher than configured security level
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task SecurityLevelConfigured_AuthenticatedUserCallingAPIWithSecurityLevelInTokenThatIsHigherThanConfigured_Return200()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create.Default().WithSecurityLevel(["3"]);
            var (accessToken, idToken) = await CreateAccessAndIdToken(config.ClientId, config.AllScopes.ToList(), securityLevel: "4");

            var appSettings = config.CreateConfigurationRoot();
            var app = CreateApplicationBuilderWithConfiguration(appSettings)
               .WithServices(ConfigureHelseIdAuthenticationAndFakeAuthentication(accessToken, idToken, appSettings))
               .BuildApp(UseEnpointAuthenticationAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        /// <summary>
        /// Security level in token is lower than configured security level
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task SecurityLevelConfigured_AuthenticatedUserCallingAPIWithSecurityLevelInTokenThatIsLowerThanConfigured_Return403()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create.Default().WithSecurityLevel(["4"]);
            var (accessToken, idToken) = await CreateAccessAndIdToken(config.ClientId, config.AllScopes.ToList(), securityLevel: "2");

            var appSettings = config.CreateConfigurationRoot();
            var app = CreateApplicationBuilderWithConfiguration(appSettings)
                .WithServices(ConfigureHelseIdAuthenticationAndFakeAuthentication(accessToken, idToken, appSettings))
                .BuildApp(UseEnpointAuthenticationAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        private static async Task<(string AccessToken, string IdToken)> CreateAccessAndIdToken(
          string clientId,
          IEnumerable<string> scopes,
          string audience = "fhi:api",
          string? securityLevel = "4")
        {
            var accessToken = await TTTService.GetHelseIdToken(TTTTokenRequests.DefaultAccessToken(scopes.ToList(), audience));
            var idToken = await TTTService.GetHelseIdToken(TTTTokenRequests.IdToken(clientId, scopes.ToList(), securityLevel: securityLevel));

            return (accessToken, idToken);
        }

        private static Action<IServiceCollection> ConfigureHelseIdAuthenticationAndFakeAuthentication(string accessToken, string idToken, IConfigurationRoot appSettings)
        {
            return services =>
            {
                services.AddFakeTestAuthenticationScheme(accessToken, idToken);
                services.AddHelseIdWebAuthentication(appSettings).Build();
            };
        }

        private static WebApplicationBuilder CreateApplicationBuilderWithConfiguration(IConfigurationRoot appSettings)
        {
            return WebApplicationBuilderTestHost.CreateWebHostBuilder().WithConfiguration(appSettings);
        }

        private static Action<WebApplication> UseEnpointAuthenticationAuthorization()
        {
            return app =>
            {
                app.UseRouting();
                app.MapGet("/api/test-endpoint",
                    [Authorize]
                () => "Hello world!");
                app.UseAuthentication();
                app.UseAuthorization();
            };
        }
    }
}
