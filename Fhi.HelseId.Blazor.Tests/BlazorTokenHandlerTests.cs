using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Fhi.HelseId.Blazor.Tests;

public class BlazorTokenHandlerTests
{
    [Test]
    public async Task HandlerAddsAuthToken()
    {
        var authToken = Guid.NewGuid().ToString();

        var tokenService = Substitute.For<IBlazorTokenService>();
        tokenService.GetToken().Returns(Task.FromResult((string?)authToken));

        var handler = new BlazorTokenHandler(tokenService);
        handler.InnerHandler = new DummyInnerHandler();

        var client = new HttpClient(handler);
        var response = await client.GetAsync("http://localhost/");

        var token = await response.Content.ReadAsStringAsync();

        // the dummy client returns the authorization header
        Assert.That(token, Is.EqualTo("Bearer " + authToken));
    }

    [Test]
    public async Task HandlerSkipsAuthIfAnonymousOptionIsSet()
    {
        var tokenService = Substitute.For<IBlazorTokenService>();
        tokenService.GetToken().Throws(new Exception("Should not try to get token"));

        var handler = new BlazorTokenHandler(tokenService);
        handler.InnerHandler = new DummyInnerHandler();

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
        request.Options.TryAdd("Anonymous", "Anonymous");
        var response = await client.SendAsync(request);

        var token = await response.Content.ReadAsStringAsync();

        // the dummy client returns the authorization header
        Assert.That(token, Is.EqualTo(""));
    }
}
