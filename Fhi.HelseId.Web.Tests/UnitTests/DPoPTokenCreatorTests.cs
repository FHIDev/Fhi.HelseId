using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Fhi.HelseId.Common.DPoP;
using Fhi.HelseId.Web.DPoP;
using Fhi.HelseId.Web.Services;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Web.UnitTests;

internal class DPoPTokenCreatorTests
{
    private INonceStore? _nonceStore;
    private DPoPTokenCreator? _dPoPTokenCreator;
    private IHelseIdClientSecretHandler? _secretHandler;

    [SetUp]
    public void SetUp()
    {
        var rsaKey = new RsaSecurityKey(RSA.Create(2048));
        var jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaKey);

        _nonceStore = Substitute.For<INonceStore>();
        _secretHandler = Substitute.For<IHelseIdClientSecretHandler>();
        _secretHandler.GetSecurityKey().Returns(jsonWebKey);

        _dPoPTokenCreator = new DPoPTokenCreator(_nonceStore, _secretHandler);
    }

    [Test]
    public async Task CreateSignedToken_CreatesValidToken_WithNonce()
    {
        // Arrange
        var method = HttpMethod.Get;
        var url = "https://example.com";
        var nonce = "test-nonce";
        _nonceStore!.SetNonce(url, method.ToString(), nonce).Returns(Task.CompletedTask);

        // Act
        var token = await _dPoPTokenCreator!.CreateSignedToken(method, url, nonce);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.That(jwtToken, Is.Not.Null);
        Assert.That(jwtToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Nonce && c.Value == nonce), Is.True);
    }

    [Test]
    public async Task CreateSignedToken_CreatesValidToken_WithoutNonce()
    {
        // Arrange
        var method = HttpMethod.Get;
        var url = "https://example.com";
        var generatedNonce = "generated-nonce";
        _nonceStore!.GetNonce(url, method.ToString()).Returns(Task.FromResult(generatedNonce));

        // Act
        var token = await _dPoPTokenCreator!.CreateSignedToken(method, url);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.That(jwtToken, Is.Not.Null);
        Assert.That(jwtToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Nonce && c.Value == generatedNonce), Is.True);
    }

    [Test]
    public async Task CreateSignedToken_ContainsExpectedClaims()
    {
        // Arrange
        var method = HttpMethod.Post;
        var url = "https://example.com";
        var nonce = "test-nonce";
        var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        _nonceStore!.SetNonce(url, method.ToString(), nonce).Returns(Task.CompletedTask);

        // Act
        var token = await _dPoPTokenCreator!.CreateSignedToken(method, url, nonce);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.That(jwtToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Jti), Is.True);
        Assert.That(jwtToken.Claims.Any(c => c.Type == DPoPClaimNames.HttpMethod && c.Value.Equals(method.ToString(), StringComparison.InvariantCultureIgnoreCase)), Is.True);
        Assert.That(jwtToken.Claims.Any(c => c.Type == DPoPClaimNames.HttpUrl && c.Value == url), Is.True);
        Assert.That(jwtToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Iat && c.Value == iat), Is.True);
    }

    [Test]
    public async Task CreateSignedToken_SetsJwtHeader_WithCorrectTypeAndJwk()
    {
        // Arrange
        var method = HttpMethod.Get;
        var url = "https://example.com";
        var nonce = "test-nonce";
        _nonceStore!.SetNonce(url, method.ToString(), nonce).Returns(Task.CompletedTask);

        // Act
        var token = await _dPoPTokenCreator!.CreateSignedToken(method, url, nonce);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var jwk = new JsonWebKey(jwtToken.Header[JwtHeaderParameterNames.Jwk].ToString());

        Assert.That(jwtToken.Header[JwtHeaderParameterNames.Typ], Is.EqualTo("dpop+jwt"));
        Assert.That(jwtToken.Header[JwtHeaderParameterNames.Jwk], Is.Not.Null);
        Assert.That(jwk.Alg, Is.EqualTo("PS256"));
    }
}
