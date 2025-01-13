using Fhi.HelseId.Api.DPoP;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace Fhi.HelseId.Api.UnitTests;

internal class DPoPExtensionsTests
{
    private const string DPoPToken = "dpop-token";
    private const string DPoPProof = "dpop-proof";
    private HttpRequest? _httpRequestMock;

    [SetUp]
    public void Setup()
    {
        _httpRequestMock = Substitute.For<HttpRequest>();
    }

    [Test]
    public void TryGetDPoPAccessToken_AuthorizationHeaderExists_ReturnsTrueAndToken()
    {
        // Arrange
        _httpRequestMock!.Headers.Authorization = $"DPoP {DPoPToken}";

        // Act
        var result = _httpRequestMock.TryGetDPoPAccessToken(out var token);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Is.True);
            Assert.That(token, Is.EqualTo(DPoPToken));
        });
    }

    [Test]
    public void TryGetDPoPAccessToken_AuthorizationHeaderExists_ReturnsTrueAndToken_NotCaseSensitive()
    {
        // Arrange
        _httpRequestMock!.Headers.Authorization = $"DPOP {DPoPToken}";

        // Act
        var result = _httpRequestMock.TryGetDPoPAccessToken(out var token);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Is.True);
            Assert.That(token, Is.EqualTo(DPoPToken));
        });
    }

    [Test]
    public void TryGetDPoPAccessToken_AuthorizationHeaderMissing_ReturnsFalseAndEmptyToken()
    {
        // Arrange
        _httpRequestMock!.Headers.Authorization = StringValues.Empty;

        // Act
        var result = _httpRequestMock.TryGetDPoPAccessToken(out var token);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Is.False);
            Assert.That(token, Is.Empty);
        });
    }

    [Test]
    public void TryGetDPoPAccessToken_AuthorizationHeaderWrongSchema_ReturnsFalseAndEmptyToken()
    {
        // Arrange
        _httpRequestMock!.Headers.Authorization = "Bearer invalid-token";

        // Act
        var result = _httpRequestMock.TryGetDPoPAccessToken(out var token);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Is.False);
            Assert.That(token, Is.Empty);
        });
    }

    [Test]
    public void TryGetDPoPProof_DPoPProofHeaderExists_ReturnsTrueAndProof()
    {
        // Arrange
        _httpRequestMock!.Headers["DPoP"] = DPoPProof;

        // Act
        var result = _httpRequestMock.TryGetDPoPProof(out var proof);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Is.True);
            Assert.That(proof, Is.EqualTo(DPoPProof));
        });
    }

    [Test]
    public void TryGetDPoPProof_DPoPProofHeaderMissing_ReturnsFalseAndEmptyProof()
    {
        // Arrange
        _httpRequestMock!.Headers["DPoP"] = null as string;

        // Act
        var result = _httpRequestMock.TryGetDPoPProof(out var proof);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Is.False);
            Assert.That(proof, Is.Empty);
        });
    }

    [Test]
    public void CreateThumbprint_ValidJwk_ReturnsExpectedThumbprint()
    {
        // Arrange
        var jwk = new JsonWebKey
        {
            Kty = "RSA",
            N = "test-modulus",
            E = "test-exponent"
        };
        var expectedThumbprint = Base64UrlEncoder.Encode(jwk.ComputeJwkThumbprint());

        // Act
        var thumbprint = jwk.CreateThumbprint();

        // Assert
        Assert.That(thumbprint, Is.EqualTo(expectedThumbprint));
    }
}
