using System.Net;
using System.Text.Json;
using Fhi.ClientCredentialsKeypairs;
using Fhi.HelseId.Api.ExtensionMethods;
using Fhi.HelseId.Integration.Tests.HelseId.Api.Setup;
using Fhi.TestFramework;

namespace Fhi.HelseId.Api.IntegrationTests;

/// <summary>
/// Puropse of these tests is to verify that RequireDPoPTokens setting works as intended. It should not accept JWT access_token when DPoP is required.
/// </summary>
public class DPoPTests
{
    [Test]
    public async Task ApiCallWithDpopToken_ApiAcceptsBothDpopAndBearer_Returns200Ok()
    {
        var config = CreateConfig()
            .WithAllowDPoPTokens(true)
            .WithRequireDPoPTokens(false);

        var client = CreateDirectHttpClient(config, useDpop: true);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo("Hello world!"));
        });
    }

    [Test]
    public async Task ApiCallWithBearerToken_ApiAcceptsBothDpopAndBearer_Returns200Ok()
    {
        var config = CreateConfig()
            .WithAllowDPoPTokens(true)
            .WithRequireDPoPTokens(false);

        using var client = CreateDirectHttpClient(config, useDpop: false);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo("Hello world!"));
        });
    }

    /// <summary>
    /// TODO: should use WWW-Authenticate header and check for token not validate.
    /// </summary>
    [Test]
    public async Task ApiCallWithBearerToken_ApiAcceptsOnlyDPoP_THEN_Returns401()
    {
        var config = CreateConfig()
            .WithAllowDPoPTokens(true)
            .WithRequireDPoPTokens(true);

        using var client = CreateDirectHttpClient(config, useDpop: false);
        var response = await client.GetAsync("api/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    private static HttpClient CreateDirectHttpClient(HelseIdApiKonfigurasjon apiConfig, bool useDpop = true)
    {
        var configString = File.ReadAllText("IntegrationTests/Fhi.HelseId.Testing.Api.json");
        var config = JsonSerializer.Deserialize<ClientCredentialsConfiguration>(configString)
            ?? throw new Exception("No config found in Fhi.HelseId.Testing.Api.json");

        var factory = new WebApplicationFactoryTestHost<Program>(services =>
        {
            services.AddHelseIdApiAuthentication(apiConfig);
            services.AddHelseIdAuthorizationControllers(apiConfig);
        });

        var client = factory.CreateClient();
        var handler = BuildProvider(config, useDpop);
        return factory.CreateDefaultClient(factory.ClientOptions.BaseAddress, handler);
    }

    private static HttpAuthHandler BuildProvider(ClientCredentialsConfiguration config, bool useDpop)
    {
        var apiConfig = new ClientCredentialsKeypairs.Api
        {
            UseDpop = useDpop
        };
        var store = new AuthenticationService(config, apiConfig);
        var tokenProvider = new AuthenticationStore(store, config);
        var authHandler = new HttpAuthHandler(tokenProvider);
        return authHandler;
    }

    /// <summary>
    /// Create HelseId with values required by all clients.
    /// The CreateDirectHttpClient creates token by TTT with fhi:helseid.testing.api scope so audience and allowed.
    /// scope must be set to not get 401 with invalid_token.
    /// </summary>
    /// <returns>HelseIdApiKonfigurasjon with default values for test use</returns>
    private static HelseIdApiKonfigurasjon CreateConfig()
    {
        var audienceSetInTTTgeneratedToken = "fhi:helseid.testing.api";
        var scopeSetInTTTgeneratedToken = "fhi:helseid.testing.api/all";

        return HelseIdApiKonfigurasjonBuilder
            .Create
            .DefaultValues(audience: audienceSetInTTTgeneratedToken, allowedScopes: scopeSetInTTTgeneratedToken);
    }
}
