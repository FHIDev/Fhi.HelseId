using System.Net;
using System.Web;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.IntegrationTests.Setup;
using Fhi.TestFramework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Fhi.HelseId.Web.IntegrationTests
{
    /// <summary>
    /// The purpose of these tests is to test the buildt in OIDC authentication scheme in AddHelseIdWebAuthentication
    /// </summary>
    public class AuthenticationTests
    {
        [Test]
        public async Task DefaultHelseIdConfiguration_NoAuthCookieOnApiCall_Return401WithRedirectToIdentityProvider()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create
                .AddDefaultValues()
                .CreateConfigurationRoot();

            var app = WebApplicationBuilderTestHost.CreateWebHostBuilder()
                .WithConfiguration(config)
                .WithServices(services =>
                {
                    services.AddHelseIdWebAuthentication(config)
                   .Build();
                })
                .BuildApp(UseEndpointAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/testauthentication");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            var queryParams = HttpUtility.ParseQueryString(response.Headers.Location!.Query);
            Assert.That(queryParams["scope"], Is.EqualTo("openid profile helseid://scopes/identity/pid helseid://scopes/identity/pid_pseudonym helseid://scopes/identity/security_level offline_access"));
            Assert.That(queryParams["redirect_uri"], Is.EqualTo("http://localhost/signin-callback"));
        }

        [Test]
        public async Task ScopeConfigured_NoAuthCookieOnApiCall_Return401WithRedirectToIdentityProvider()
        {
            var helseIdConfig = HelseIdWebKonfigurasjonBuilder.Create
                .AddDefaultValues();
            helseIdConfig.Scopes = ["aditional_scope"];
            var config = helseIdConfig.CreateConfigurationRoot();

            var app = WebApplicationBuilderTestHost.CreateWebHostBuilder()
                .WithConfiguration(config)
                .WithServices(services =>
                {
                    services.AddHelseIdWebAuthentication(config)
                   .Build();
                })
                .BuildApp(UseEndpointAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/testauthentication");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            var queryParams = HttpUtility.ParseQueryString(response.Headers.Location!.Query);
            Assert.That(queryParams["scope"], Is.EqualTo("openid profile helseid://scopes/identity/pid helseid://scopes/identity/pid_pseudonym helseid://scopes/identity/security_level offline_access"));
            Assert.That(queryParams["redirect_uri"], Is.EqualTo("http://localhost/signin-callback"));
        }

        /// <summary>
        /// Test signin callback with token
        /// https://github.com/dotnet/aspnetcore/blob/81a2bab8704d87d324039b42eb1bab0d977f25b8/src/Security/Authentication/test/OpenIdConnect/OpenIdConnectEventTests_Handler.cs
        /// </summary>
        /// <returns></returns>
        [Test]
        [Ignore("Will be implemented later")]
        public async Task DefaultHelseIdConfiguration_InvalidTokenRecieved_()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create.AddDefaultValues();
            var configRoot = config.CreateConfigurationRoot();
            var app = WebApplicationBuilderTestHost.CreateWebHostBuilder()
                .WithConfiguration(configRoot)
                .WithServices(services =>
                {
                    services.AddHelseIdWebAuthentication(configRoot)
                   .Build();
                })
                .BuildApp(UseEndpointAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.PostAsync("/signin-callback", null);
        }

        private static Action<WebApplication> UseEndpointAuthenticationAndAuthorization()
        {
            return app =>
            {
                app.UseRouting();
                app.MapGet("/api/testauthentication",
                    [Authorize]
                () => "Hello world!");
                app.UseAuthentication();
                app.UseAuthorization();
            };
        }
    }
}




