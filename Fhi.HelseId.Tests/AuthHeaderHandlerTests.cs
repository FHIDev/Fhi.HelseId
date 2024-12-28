using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Fhi.HelseId.Common;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests;

// TODO: remove? What do we want to achieve with these tests?
public class AuthHeaderHandlerTests
{
    [Test]
    public async Task HandlerAddsAuthToken()
    {
        var authToken = Guid.NewGuid().ToString();
        var client = SetupInfrastructure(authToken);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");

        var response = await client.SendAsync(request);

        // The dummy client returns the authorization header value if any
        var token = await response.Content.ReadAsStringAsync();
        Assert.That(token, Is.EqualTo("Bearer " + authToken));
    }

    [Test]
    public async Task HandlerDoesNotGetTokenWhenAnonymous()
    {
        var authToken = Guid.NewGuid().ToString();
        var client = SetupInfrastructure(authToken);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
        request.Options.TryAdd("Anonymous", "Anonymous");

        var response = await client.SendAsync(request);

        // The dummy client returns the authorization header value if any
        var token = await response.Content.ReadAsStringAsync();
        Assert.That(token, Is.EqualTo(""));
    }

    private HttpClient SetupInfrastructure(string authToken)
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = Substitute.For<HttpContext>();
        var services = new ServiceCollection();
        var authService = Substitute.For<IAuthenticationService>();
        var claimsPrincipal = new ClaimsPrincipal();
        var authenticationProperties = new AuthenticationProperties();
        authenticationProperties.StoreTokens([new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = authToken
            }
        ]);
        var authResult = AuthenticateResult.Success(new AuthenticationTicket(
            claimsPrincipal,
            authenticationProperties,
            JwtBearerDefaults.AuthenticationScheme));
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string>()).Returns(authResult);

        services.AddSingleton(authService);

        var serviceProvider = services.BuildServiceProvider();
        context.RequestServices.Returns(serviceProvider);
        httpContextAccessor.HttpContext.Returns(context);

        var handler = new AuthHeaderHandler(
            httpContextAccessor,
            NullLogger<AuthHeaderHandler>.Instance,
            Substitute.For<ICurrentUser>(),
            Options.Create(new HelseIdWebKonfigurasjon()),
            new BearerAuthorizationHeaderSetter());

        handler.InnerHandler = new DummyInnerHandler();

        return new HttpClient(handler);
    }
}

internal class DummyInnerHandler : HttpClientHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get any auth header, and use its value as the content of the response
        var auth = request.Headers.FirstOrDefault(x => x.Key == "Authorization").Value?.FirstOrDefault() ?? "";
        var response = new HttpResponseMessage() { Content = new StringContent(auth) };

        return Task.FromResult(response);
    }
}