using System.Net;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.TestFramework;
using Fhi.TestFramework.AuthenticationSchemes.CookieAuthenticationScheme;
using Fhi.TestFramework.Extensions;
using Fhi.TestFramework.NHNTTT;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Fhi.TestFramework.Samples.Authentication
{
    internal class FakeAuthenticationSamples
    {
        /// <summary>
        /// Specify user claims
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task FakeAuthenticationWithClaims_ApiEndpointCalled_ReturnOk()
        {
            var builder = WebApplicationBuilderTestHost.CreateWebHostBuilder()
             .WithServices(services =>
             {
                 services.AddFakeTestAuthenticationScheme(
                 [
                     new(IdentityClaims.Name, "Line Danser")
                 ]);
                 services.AddAuthorization();
             });

            var app = builder.BuildApp(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapGet("/api/test-endpoint", [Authorize] async (context) =>
                {
                    var name = context.User.Name();
                    await context.Response.WriteAsync($"{name}");
                });
            });
            app.Start();

            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(responseBody, Is.EqualTo("Line Danser"));
            });
        }

        /// <summary>
        /// Using NHN Test token tjeneste (TTT) to generate access and id token
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task FakeAuthenticationWithToken_ApiEndpointCalled_ReturnOk()
        {
            var accessToken = await TTTService.GetHelseIdToken(TTTTokenRequests.DefaultAccessToken(["fhi:api/scope1"], "fhi:api"));
            var idToken = await TTTService.GetHelseIdToken(TTTTokenRequests.IdToken(Guid.NewGuid().ToString(), ["fhi:api/scope1"]));

            var builder = WebApplicationBuilderTestHost.CreateWebHostBuilder()
             .WithServices(services =>
             {
                 services.AddFakeTestAuthenticationScheme(accessToken, idToken);
                 services.AddAuthorization();
             });

            var app = builder.BuildApp(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapGet("/api/test-endpoint", [Authorize] async (context) =>
                {
                    var name = context.User.Name();
                    await context.Response.WriteAsync($"{name}");
                });
            });
            app.Start();


            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(responseBody, Is.EqualTo("GRØNN VITS"));
            });
        }


        /// <summary>
        /// Override OIDC scheme options in HelseId web authentication
        /// </summary>
        /// <returns></returns>
        [Test]
        [Ignore("Sample of how to override OIDC events")]
        public async Task OverrideOIDCEvents()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                { "HelseIdWebKonfigurasjon:AuthUse", "false" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" }
            };
            var config = appsettingsConfig.BuildInMemoryConfiguration();
            var builder = WebApplicationBuilderTestHost.CreateWebHostBuilder()
             .WithServices(services =>
             {
                 // Sample of using HelseId library
                 services.AddHelseIdWebAuthentication(config).Build();

                 //Create a new IConfigureNamedOptions to override OIDC authentication scheme options
                 services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureOidcOptions>();
             });

            var app = builder.BuildApp(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapGet("/api/test-endpoint", [Authorize] async (context) =>
                {
                    var name = context.User.Name();
                    await context.Response.WriteAsync($"{name}");
                });
            });
            app.Start();

            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        [Ignore("Sample of how to override Cookie events")]
        public async Task OverrideCookie()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                { "HelseIdWebKonfigurasjon:AuthUse", "false" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" }
            };
            var config = appsettingsConfig.BuildInMemoryConfiguration();
            var builder = WebApplicationBuilderTestHost.CreateWebHostBuilder()
             .WithServices(services =>
             {
                 services.AddHelseIdWebAuthentication(config).Build();

                 //Simulate cookie
                 services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();
             });

            var app = builder.BuildApp(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapGet("/api/test-endpoint", [Authorize] async (context) =>
                {
                    var name = context.User.Name();
                    await context.Response.WriteAsync($"{name}");
                });
            });
            app.Start();


            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        /// <summary>
        /// Sample on overidden OIDC configuration
        /// </summary>
        internal class ConfigureOidcOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            public void Configure(string? name, OpenIdConnectOptions options)
            {
                Configure(options);
            }

            /// <summary>
            /// sample on overridden configuration. 
            /// Overriding evenst for illustration
            /// </summary>
            /// <param name="options"></param>
            public void Configure(OpenIdConnectOptions options)
            {
                options.CallbackPath = "https://customized_redirect_uri";
                options.Events.OnAuthorizationCodeReceived = ctx =>
                {
                    return Task.CompletedTask;
                };
            }
        }
    }
}
