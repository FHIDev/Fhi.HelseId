using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Constants;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.JSInterop;
using NSubstitute;
using NUnit.Framework;
using Refit;

namespace Fhi.HelseId.Blazor.Tests;

public partial class HelseidRefitBuilderForBlazorTests
{
    private const string AccessTokenValue = "test-token";
    private const string ContextCorrelationId = "context-correlation-guid";

    [Test]
    public async Task CanCreateWorkingClient()
    {
        var options = new HelseidRefitBuilderForBlazorOptions()
        {
            HttpClientHandlerBuilder = x => new DummyInnerHandler(),
        };

        var client = CreateTestClient(options, out var provider);

        var response = await client.Info();

        // the dummy client returns the authorization header
        Assert.That(response.Content, Is.EqualTo("Bearer " + AccessTokenValue));

        // the dummy client mirrors all "fhi-" headers. Let's check that they are encoded correctly
        Assert.That(response.Headers.Single(x => x.Key == ITestClient.TestHeaderName).Value.Single(),
            Is.EqualTo("test &#230;"));

        // check that the correlation id is picked up from the state
        Assert.That(response.Headers.Single(x => x.Key == CorrelationIdHandler.CorrelationIdHeaderName).Value.Single(),
        Is.EqualTo(ContextCorrelationId));

        var logger = (TestLogger<LoggingDelegationHandler>)provider.GetRequiredService<ILogger<LoggingDelegationHandler>>();
        Assert.That(logger.Entries.Any(x => x.Contains(ContextCorrelationId)), Is.True, "Correlation id not found: " + logger.Entries.First());
    }

    [Test]
    public async Task CanToggleDefaultHandlers()
    {
        var correlationId = Guid.NewGuid().ToString();

        var options = new HelseidRefitBuilderForBlazorOptions()
        {
            HttpClientHandlerBuilder = x => new DummyInnerHandler(),
            UseDefaultTokenHandler = false,
            CustomCorrelationIdFunc = p => correlationId,
            HtmlEncodeFhiHeaders = false,
        };

        var client = CreateTestClient(options, out var provider);

        var response = await client.Info();

        // the dummy client returns the authorization header which we did not provide
        Assert.That(response.Content, Is.EqualTo(""));

        // check that we get the correlation id from our own func
        Assert.That(response.Headers.Single(x => x.Key == CorrelationIdHandler.CorrelationIdHeaderName).Value.Single(),
            Is.EqualTo(correlationId));

        // the dummy client mirrors all "fhi-" headers. Let's check that we did not add the encoding-handler
        Assert.That(response.Headers.Single(x => x.Key == ITestClient.TestHeaderName).Value.Single(),
            Is.EqualTo("test æ"));
    }

    public ITestClient CreateTestClient(HelseidRefitBuilderForBlazorOptions options, out ServiceProvider provider)
    {
        var services = CreateDefaultServiceCollection();

        var config = new HelseIdWebKonfigurasjon()
        {
            Apis = [new() { Url = "http://localhost", Name = "ITestClient" }]
        };

        services.AddHelseIdForBlazor(config, options)
            .AddRefitClient<ITestClient>();

        provider = services.BuildServiceProvider();

        // Populate the HelseIdState. This would normally happen in the start of the Blazor Circuit.
        provider.GetRequiredService<HelseIdState>().Populate(CreateContext(provider)).Wait();

        return provider.GetRequiredService<ITestClient>();
    }

    private ServiceCollection CreateDefaultServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
        services.AddSingleton(Substitute.For<IJSRuntime>());

        // Add authentication parameters for a logged in user
        var authStore = Substitute.For<IAuthenticationService>();
        var authItems = new Dictionary<string, string?>()
        {
            { $".Token.{OAuthConstants.ExpiresAt}", DateTime.UtcNow.AddDays(1).ToString("u") },
            { $".Token.{OpenIdConnectParameterNames.AccessToken}", AccessTokenValue },
        };

        var ticket = AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties(authItems), ""));
        authStore.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>())
            .Returns(Task.FromResult(ticket));
        services.AddSingleton(authStore);

        // Create fake TokenEndpointService
        services.AddSingleton(new TokenEndpointService(
            Substitute.For<IOptions<AutomaticTokenManagementOptions>>(),
            Substitute.For<IOptionsSnapshot<OpenIdConnectOptions>>(),
            Substitute.For<IAuthenticationSchemeProvider>(),
            Substitute.For<HttpClient>(),
            Substitute.For<IHttpContextAccessor>(),
            NullLogger<TokenEndpointService>.Instance,
            Substitute.For<IHelseIdClientSecretHandler>()));

        return services;
    }

    public HttpContext CreateContext(IServiceProvider provider)
    {
        var context = Substitute.For<HttpContext>();
        context.RequestServices.Returns(provider);
        var request = Substitute.For<HttpRequest>();
        context.Request.Returns(request);

        var headers = new HeaderDictionary();
        headers.TryAdd(CorrelationIdHandler.CorrelationIdHeaderName, ContextCorrelationId);
        request.Headers.Returns(headers);

        return context;
    }

    public interface ITestClient
    {
        public const string TestHeaderName = "fhi-encoded";

        [Get("/info")]
        [Headers($"{TestHeaderName}: test æ")]
        Task<ApiResponse<string>> Info();
    }
}
