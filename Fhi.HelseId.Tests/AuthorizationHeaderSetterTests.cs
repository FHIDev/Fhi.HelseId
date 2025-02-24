using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fhi.HelseId.Common.DPoP;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.DPoP;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests;

public class AuthorizationHeaderSetterTests
{
    private HttpRequestMessage _request = null!;

    [SetUp]
    public void SetUp()
    {
        _request = new HttpRequestMessage(HttpMethod.Get, "https://example.com?whatever");
    }

    [TearDown]
    public void Dispose()
    {
        _request.Dispose();
    }

    [Test]
    public async Task BearerAuthorizationHeaderSetter_ShouldSetAuthorizationHeader()
    {
        // Arrange
        var token = "test-token";
        var bearerHeaderSetter = new BearerAuthorizationHeaderSetter();

        // Act
        await bearerHeaderSetter.SetAuthorizationHeader(_request, token);

        // Assert
        Assert.That(_request.Headers.Authorization, Is.Not.Null);
        Assert.That(_request.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
        Assert.That(_request.Headers.Authorization!.Parameter, Is.EqualTo(token));
    }

    [Test]
    public async Task DPoPAuthorizationHeaderSetter_ShouldSetAuthorizationHeaderAndProofHeader()
    {
        // Arrange
        var token = "test-token";
        var athValue = AccessTokenHash.Sha256(token);
        var proofToken = "proof-token";

        var dPoPTokenCreator = Substitute.For<IDPoPTokenCreator>();
        var expectedRequestUri = _request.RequestUri!.Scheme + "://" + _request.RequestUri!.Authority +
                                 _request.RequestUri!.LocalPath;
        dPoPTokenCreator.CreateSignedToken(_request.Method, expectedRequestUri, ath: athValue)
            .Returns(Task.FromResult(proofToken));

        var dPoPHeaderSetter = new DPoPAuthorizationHeaderSetter(dPoPTokenCreator);

        // Act
        await dPoPHeaderSetter.SetAuthorizationHeader(_request, token);

        // Assert
        Assert.That(_request.Headers.Authorization, Is.Not.Null);
        Assert.That(_request.Headers.Authorization!.Scheme, Is.EqualTo("DPoP"));
        Assert.That(_request.Headers.Authorization!.Parameter, Is.EqualTo(token));
        Assert.That(_request.Headers.Contains(DPoPHttpHeaders.ProofHeaderName), Is.True);
        Assert.That(_request.Headers.GetValues(DPoPHttpHeaders.ProofHeaderName).FirstOrDefault(), Is.EqualTo(proofToken));
    }
}
