using System.Net;
using Fhi.HelseId.Api;
using Fhi.HelseId.Api.ExtensionMethods;
using Fhi.HelseId.Integration.Tests.HelseId.Api.Setup;
using Fhi.TestFramework;
using Fhi.TestFramework.Extensions;
using Fhi.TestFramework.NHNTTT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseId.Api.IntegrationTests;

/// <summary>
/// Purpose of the test is to if multi scope and single scope authorization of configured API scopes works.
/// </summary>
public class ScopeTests
{
    [Test]
    public async Task SingleScopeIsConfigured_RequestContainsTokenTokenWithTheSingleScope_Returns200Ok()
    {
        var config = HelseIdApiKonfigurasjonBuilder.Create.DefaultValues(audience: "fhi:helseid.testing.api").WithAllowedScopes("fhi:helseid.testing.api");
        var accessToken = await GetTestToken(["fhi:helseid.testing.api"], config.ApiName);

        var app = CreateWebApplication(config);
        app.Start();
        var client = app.GetTestClient().AddBearerAuthorizationHeader(accessToken);
        var response = await client.GetAsync("api/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task MultipleScopesIsconfigured_RequestContainsTokenWithOneOfTheConfiguredScopes_Returns200Ok()
    {
        var audience = "fhi:helseid.testing.api";
        var config = HelseIdApiKonfigurasjonBuilder.Create.DefaultValues(audience: audience).WithAllowedScopes($"{audience}/all,{audience}/person");
        var accessToken = await GetTestToken([$"{audience}/all"], config.ApiName);

        var app = CreateWebApplication(config);
        app.Start();
        var client = app.GetTestClient().AddBearerAuthorizationHeader(accessToken);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo("Hello world!"));
        });
    }

    [Test]
    public async Task MultipleScopesIsconfigured_RequestContainsTokenWithAllOfTheConfiguredScopes_Returns200Ok()
    {
        var audience = "fhi:helseid.testing.api";
        var config = HelseIdApiKonfigurasjonBuilder.Create.DefaultValues(audience: audience).WithAllowedScopes($"{audience}/all,{audience}/person");
        var accessToken = await GetTestToken([$"{audience}/all", $"{audience}/person"], config.ApiName);

        var app = CreateWebApplication(config);
        app.Start();
        var client = app.GetTestClient().AddBearerAuthorizationHeader(accessToken);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo("Hello world!"));
        });
    }

    /// <summary>
    /// This case is also a bit strange since audience will validate ok. The audience of an access_token.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task MultipleScopesIsconfigured_RequestContainsTokenDoesNotContainAnyOfTheConfiguredScopes_Returns403()
    {
        var audience = "fhi:api-scope";
        var config = HelseIdApiKonfigurasjonBuilder.Create.DefaultValues(audience: audience).WithAllowedScopes($"{audience}/all,{audience}/person");
        var accessToken = await GetTestToken(["fhi:non-valid-scope"], audience);

        var app = CreateWebApplication(config);
        app.Start();
        var client = app.GetTestClient().AddBearerAuthorizationHeader(accessToken);
        var response = await client.GetAsync("api/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private static WebApplication CreateWebApplication(HelseIdApiKonfigurasjon config)
    {
        return WebApplicationBuilderTestHost.CreateWebHostBuilder()
                .WithServices(services =>
                {
                    services.AddHelseIdApiAuthentication(config);
                    services.AddHelseIdAuthorizationControllers(config);
                })
                .BuildApp(app =>
                {
                    app.MapGet("/api/test",
                    [Authorize]
                    () => "Hello world!");
                    app.UseHttpsRedirection();
                    app.UseAuthorization();
                    app.MapControllers();
                });
    }

    private static Task<string> GetTestToken(ICollection<string> scopes, string audience)
    {
        return TTTService.GetHelseIdToken(TTTTokenRequests.DefaultAccessToken(scopes, audience));
    }
}
