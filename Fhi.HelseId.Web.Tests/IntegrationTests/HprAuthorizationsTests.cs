using System.Net;
using System.Security.Claims;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.Common;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Hpr.Core;
using Fhi.HelseId.Web.IntegrationTests.Setup;
using Fhi.TestFramework;
using Fhi.TestFramework.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using static ApprovalResponse;

namespace Fhi.HelseId.Web.IntegrationTests
{
    /// <summary>
    /// Helthpersonel has HPR number and have different types of authorization such as sykepleier, lege, psykolog. These Hpr number and authorization details
    /// will be added to the token if requested scope hpr_number and/or hpr_details and can be used as a basis for authorize the user.
    /// - When RequireHprNumber is set the user must have an HprNumber unless it is in an whitelist
    /// - When RequireValidHprAuthorization is set it will check if the user has valid authorization based on provided list
    /// </summary>
    public class HprAuthorizationTests
    {
        [Test]
        public async Task RequireHprNumberIsTrue_AuthenticatedUserHasValidHprNumber_ValidateHprValuesAndReturnOk()
        {
            // Arrange
            var userClaims = UserClaimsBuilder.Create()
                .WithSecurityLevel("4")
                .WithName("Line Danser")
                .WithHprNumber("Hprnr1234")
                .Build();

            var config = HelseIdWebKonfigurasjonBuilder.Create
                .Default()
                .WithRequireHprNumber(true)
                .WithRequireValidHprAuthorization(false)
                .CreateConfigurationRoot();

            var app = WebApplicationBuilderTestHost.CreateWebHostBuilder()
                .WithConfiguration(config)
                .WithServices(services =>
                {
                    services.AddSingleton<IAuthorizationMiddlewareResultHandler, FakeAuthorizationResultHandler>();
                    services.AddFakeTestAuthenticationScheme(userClaims);
                    services.AddHelseIdWebAuthentication(config).Build();
                })
                .BuildApp(WithAuthenticationAndAuthorization());


            // Act
            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = (await response.Content.ReadAsStringAsync()).Deserialize<AuthorizationResponse>();
            Assert.Multiple(() =>
            {
                Assert.That(content!.UserName, Is.EqualTo("Line Danser"));
                Assert.That(content.Requirements.Count, Is.EqualTo(3));
                Assert.That(content.Requirements, Does.Contain("HprAuthorizationRequirement"));
                Assert.That(content.Requirements, Does.Contain("SecurityLevelOrApiRequirement"));
                Assert.That(content.Requirements, Does.Contain("DenyAnonymousAuthorizationRequirement"));
            });
        }

        [Test]
        public async Task RequireHprNumberIsTrue_AuthenticatedUserMissingHprNumber_ValidateHprvaluesAndReturnForbidden()
        {
            var userClaims = UserClaimsBuilder.Create()
                 .WithSecurityLevel("4")
                 .WithName("Line Danser")
                 .Build();

            var config = HelseIdWebKonfigurasjonBuilder.Create
               .Default()
               .WithRequireHprNumber(true)
               .WithRequireValidHprAuthorization(false);

            var appSettings = config.CreateConfigurationRoot();
            var app = WebApplicationBuilderTestHost
                .CreateWebHostBuilder()
                .WithConfiguration(appSettings)
                .WithServices(services =>
                {
                    services.AddSingleton<IAuthorizationMiddlewareResultHandler, FakeAuthorizationResultHandler>();
                    services.AddFakeTestAuthenticationScheme(userClaims);
                    services.AddHelseIdWebAuthentication(appSettings).Build();
                })
                .BuildApp(WithAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task RequireHprNumberIsFalse_AuthenticatedUserMissingHprNumber_ValidateHprvaluesAndReturnOk()
        {
            // Arrange
            var userClaims = UserClaimsBuilder.Create()
                .WithSecurityLevel("4")
                .WithName("Line Danser")
                .Build();

            var config = HelseIdWebKonfigurasjonBuilder.Create
                .Default()
                .WithRequireHprNumber(false)
                .WithRequireValidHprAuthorization(false);

            var appSettings = config.CreateConfigurationRoot();
            var app = WebApplicationBuilderTestHost
                .CreateWebHostBuilder()
                .WithConfiguration(appSettings)
                .WithServices(services =>
                {
                    services.AddSingleton<IAuthorizationMiddlewareResultHandler, FakeAuthorizationResultHandler>();
                    services.AddFakeTestAuthenticationScheme(userClaims);
                    services.AddHelseIdWebAuthentication(appSettings).Build();
                }).BuildApp(WithAuthenticationAndAuthorization());

            // Act
            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = (await response.Content.ReadAsStringAsync()).Deserialize<AuthorizationResponse>();
            Assert.Multiple(() =>
            {
                Assert.That(content!.UserName, Is.EqualTo("Line Danser"));
                Assert.That(content.Requirements.Count, Is.EqualTo(2));
                Assert.That(content.Requirements, Does.Contain("SecurityLevelOrApiRequirement"));
                Assert.That(content.Requirements, Does.Contain("DenyAnonymousAuthorizationRequirement"));
            });
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        public async Task RequireValidHprAuthorization_AuthenticatedUserHasValidHprNumberAndIsApproved_ValidateHprNumberAndHprAuthorizationReturnOk(bool requireHprNumber, bool requireValidHprAuthorization)
        {
            var userClaims = UserClaimsBuilder.Create()
               .WithSecurityLevel("4")
               .WithName("Line Danser")
               .WithHprNumber("hpr123")
               .WithHprDetails(Kodekonstanter.OId9060Sykepleier, "SP", "sykepleier")
               .Build();

            var config = HelseIdWebKonfigurasjonBuilder.Create
              .Default()
              .WithRequireHprNumber(requireHprNumber)
              .WithRequireValidHprAuthorization(requireValidHprAuthorization);

            var appSettings = config.CreateConfigurationRoot();
            var app = WebApplicationBuilderTestHost
                .CreateWebHostBuilder()
                .WithConfiguration(appSettings)
                .WithServices(services =>
                {
                    services.AddSingleton<IAuthorizationMiddlewareResultHandler, FakeAuthorizationResultHandler>();
                    services.AddFakeTestAuthenticationScheme(userClaims);
                    services.AddHelseIdWebAuthentication(appSettings).Build();
                
                    var godKjenninger = new GodkjenteHprKategoriListe();
                    godKjenninger.Add(Kodekonstanter.OId9060Sykepleier);
                    services.AddSingleton<IGodkjenteHprKategoriListe>(godKjenninger);
                })
                .BuildApp(WithAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = (await response.Content.ReadAsStringAsync()).Deserialize<AuthorizationResponse>();
            Assert.Multiple(() =>
            {
                Assert.That(content!.UserName, Is.EqualTo("Line Danser"));
                Assert.That(content.Requirements.Count, Is.EqualTo(4));
                Assert.That(content.Requirements.Contains("HprAuthorizationRequirement"));
                Assert.That(content.Requirements.Contains("HprGodkjenningAuthorizationRequirement"));
                Assert.That(content.Requirements.Contains("SecurityLevelOrApiRequirement"));
                Assert.That(content.Requirements.Contains("DenyAnonymousAuthorizationRequirement"));
            });
        }

       
        [Test]
        public async Task RequireValidHprAuthorization_AuthenticatedUserHasValidHprNumberHprDetailclaimIsMissing_ValidateHprNumberAndHprAuthorizationReturnForbidden()
        {
            var userClaims = UserClaimsBuilder.Create()
              .WithSecurityLevel("4")
              .WithName("Line Danser")
              .WithHprNumber("hpr123")
              .Build();

            var config = HelseIdWebKonfigurasjonBuilder.Create
              .Default()
              .WithRequireHprNumber(true)
              .WithRequireValidHprAuthorization(true);

            var appSettings = config.CreateConfigurationRoot();
            var app = WebApplicationBuilderTestHost
                .CreateWebHostBuilder()
                .WithConfiguration(appSettings)
                .WithServices(services =>
                {
                    services.AddSingleton<IAuthorizationMiddlewareResultHandler, FakeAuthorizationResultHandler>();
                    services.AddFakeTestAuthenticationScheme(userClaims);
                    services.AddHelseIdWebAuthentication(appSettings).Build();

                    var godKjenninger = new GodkjenteHprKategoriListe();
                    godKjenninger.Add(Kodekonstanter.OId9060Sykepleier);
                    services.AddSingleton<IGodkjenteHprKategoriListe>(godKjenninger);
                })
                .BuildApp(WithAuthenticationAndAuthorization());

            app.Start();
            var client = app.GetTestClient();
            var response = await client.GetAsync("/api/test-endpoint");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            var content = (await response.Content.ReadAsStringAsync()).Deserialize<AuthorizationResponse>();
            Assert.Multiple(() =>
            {
                Assert.That(content!.UserName, Is.EqualTo("Line Danser"));
                Assert.That(content.Requirements.Count, Is.EqualTo(4));
                Assert.That(content.Requirements.Contains("HprAuthorizationRequirement"));
                Assert.That(content.Requirements.Contains("HprGodkjenningAuthorizationRequirement"));
                Assert.That(content.Requirements.Contains("SecurityLevelOrApiRequirement"));
                Assert.That(content.Requirements.Contains("DenyAnonymousAuthorizationRequirement"));
            });
        }

        [Test]
        public void RequireValidHprAuthorizationOrHprNumper_AuthenticatedUserIsNotApprovedButIsWhitelisted_ReturnOk()
        {
        }

        private static Action<WebApplication> WithAuthenticationAndAuthorization()
        {
            return app =>
            {
                app.UseRouting();
                app.MapGet("/api/test-endpoint", [Authorize] (HttpContext httpContext) =>
                {
                    return; 
                });
                app.UseAuthentication();
                app.UseAuthorization();
            };
        }
    }

    public class FakeAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
    {
        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            var requirements = policy.Requirements.Select(s => s.GetType().Name);

            if (authorizeResult.AuthorizationFailure != null && authorizeResult.AuthorizationFailure.FailCalled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                var messages = authorizeResult.AuthorizationFailure.FailureReasons.Select(r => new { Handler = r.Handler.GetType().Name, r.Message });
                await context.Response.WriteAsJsonAsync(new { FailurResons = messages, UserName = context.User.Name(), Requirements = requirements });
                return;
            }

            context.Response.OnStarting(() =>
            {
                context.Response.WriteAsJsonAsync(new { UserName = context.User.Name(), Requirements = requirements });

                return Task.CompletedTask;
            });

            await next(context);
        }
    }


    public record AuthorizationResponse(string UserName, IEnumerable<string> Requirements);

    public class TestLoggerProvider(List<string> logMessages) : ILoggerProvider
    {
        private readonly List<string> _logMessages = logMessages;

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(_logMessages);
        }

        public void Dispose() { }
    }

    public class TestLogger(List<string> logMessages) : ILogger
    {
        private readonly List<string> _logMessages = logMessages;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logMessages.Add(formatter(state, exception));
        }
    }
}