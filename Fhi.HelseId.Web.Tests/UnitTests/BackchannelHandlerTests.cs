using Fhi.HelseId.Web.DPoP;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Web.UnitTests;

internal class BackchannelHandlerTests
{
    private IHttpContextAccessor _httpContextAccessor = null!;
    private IDPoPTokenCreator _tokenHelper = null!;
    private HttpRequestMessage _request = null!;
    private DefaultHttpContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _tokenHelper = Substitute.For<IDPoPTokenCreator>();
        _request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        _context = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(_context);
    }

    [TearDown]
    public void Dispose()
    {
        _request.Dispose();
    }

    [Test]
    public async Task SendAsync_AddsDPoPHeader_When_RequestHasJktContext()
    {
        // Arrange
        _context.Items[DPoPContext.ContextKey] = true;
        _tokenHelper.CreateSignedToken(Arg.Any<HttpMethod>(), Arg.Any<string>(), Arg.Any<string?>())
            .Returns(Task.FromResult("dpop-proof"));

        var handler = new TestableBackchannelHandler(_httpContextAccessor, _tokenHelper);

        // Act
        var response = await handler.SendTestAsync(_request);

        // Assert
        Assert.That(_request.Headers.Contains("DPoP"), Is.True);
        Assert.That(_request.Headers.GetValues("DPoP").First(), Is.EqualTo("dpop-proof"));
    }

    [Test]
    public async Task SendAsync_DoesNotAddDPoPHeader_When_RequestDoesNotHaveJktContext()
    {
        // Arrange
        // No "request_has_jkt_context" key in the context
        var handler = new TestableBackchannelHandler(_httpContextAccessor, _tokenHelper);

        // Act
        var response = await handler.SendTestAsync(_request);

        // Assert
        Assert.That(_request.Headers.Contains("DPoP"), Is.False);
    }
}

// Helper class to expose the SendAsync for testing without reflection
public class TestableBackchannelHandler : BackchannelHandler
{
    public TestableBackchannelHandler(IHttpContextAccessor httpContextAccessor, IDPoPTokenCreator tokenHelper)
        : base(httpContextAccessor, tokenHelper)
    {
        InnerHandler = new HttpClientHandler(); // You can set an actual handler or mock here if necessary
    }

    // Public method to expose the protected SendAsync for testing purposes
    public async Task<HttpResponseMessage> SendTestAsync(HttpRequestMessage request)
    {
        return await SendAsync(request, CancellationToken.None);
    }
}
