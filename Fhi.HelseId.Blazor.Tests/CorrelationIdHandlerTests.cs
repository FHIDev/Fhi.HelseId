using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Fhi.HelseId.Blazor.Tests;

public class CorrelationIdHandlerTests
{
    [Test]
    public async Task HandlerAddsCorrelationHeader()
    {
        var correlationId = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddSingleton(new HelseIdState() { CorrelationId = correlationId });
        var provider = services.BuildServiceProvider();

        var handler = new CorrelationIdHandler(provider, new HelseidRefitBuilderForBlazorOptions());
        handler.InnerHandler = new DummyInnerHandler();

        var client = new HttpClient(handler);
        var response = await client.GetAsync("http://localhost/");

        var token = await response.Content.ReadAsStringAsync();

        // check that we get the correlation id from HelseIdState
        Assert.That(response.Headers.Single(x => x.Key == CorrelationIdHandler.CorrelationIdHeaderName).Value.Single(),
            Is.EqualTo(correlationId));
    }

    [Test]
    public async Task HandlerAddsCorrelationHeaderFromCustomFunc()
    {
        var correlationId = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var handler = new CorrelationIdHandler(provider, new HelseidRefitBuilderForBlazorOptions()
        {
            CustomCorrelationIdFunc = p => correlationId
        });
        handler.InnerHandler = new DummyInnerHandler();

        var client = new HttpClient(handler);
        var response = await client.GetAsync("http://localhost/");

        var token = await response.Content.ReadAsStringAsync();

        // check that we get the correlation id from CustomCorrelationIdFunc
        Assert.That(response.Headers.Single(x => x.Key == CorrelationIdHandler.CorrelationIdHeaderName).Value.Single(),
            Is.EqualTo(correlationId));
    }
}
