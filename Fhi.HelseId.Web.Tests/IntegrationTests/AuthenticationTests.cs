using System.Collections.Specialized;
using System.Net;
using System.Web;
using Fhi.HelseId.Web.Common;
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
        public async Task DefaultHelseIdConfiguration_NoAuthCookieOnApiCall_Return401WithLocationToRedirectToIdentityProvider()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create
                .Default()
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
            AssertStandardScopes(queryParams);
            Assert.That(queryParams["redirect_uri"], Is.EqualTo("http://localhost/signin-callback"));
        }

        [Test]
        public async Task ScopeConfigured_NoAuthCookieOnApiCall_Return401WithLocationToRedirectToIdentityProviderAndConfiguredScopes()
        {
            var helseIdConfig = HelseIdWebKonfigurasjonBuilder.Create
                .Default();
            helseIdConfig.Scopes = ["scope1", "scope2"];
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
            AssertStandardScopes(queryParams);
            Assert.That(queryParams["scope"], Does.Contain("scope1 scope2"));
        }

      

        [Test]
        public async Task IncludeHprNumber_NoAuthCookieOnApiCall_Return401WithLocationToRedirectToIdentityProviderAndHprNumberScope()
        {
            var helseIdConfig = HelseIdWebKonfigurasjonBuilder.Create
                .Default()
                .WithIncludeHprNumber(true);

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
            AssertStandardScopes(queryParams);
            Assert.That(queryParams["scope"], Does.Contain("helseid://scopes/hpr/hpr_number"));
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
            var config = HelseIdWebKonfigurasjonBuilder.Create.Default();
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

        private static void AssertStandardScopes(NameValueCollection queryParams)
        {
            Assert.That(queryParams["scope"], Does.Contain("openid"));
            Assert.That(queryParams["scope"], Does.Contain("profile"));
            Assert.That(queryParams["scope"], Does.Contain("helseid://scopes/identity/pid"));
            Assert.That(queryParams["scope"], Does.Contain("helseid://scopes/identity/pid_pseudonym"));
            Assert.That(queryParams["scope"], Does.Contain("helseid://scopes/identity/security_level"));
            Assert.That(queryParams["scope"], Does.Contain("offline_access"));
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




