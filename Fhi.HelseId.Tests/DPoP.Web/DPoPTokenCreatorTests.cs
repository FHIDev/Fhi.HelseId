﻿using Fhi.HelseId.Web.DPoP;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fhi.HelseId.Tests.DPoP.Web;

internal class DPoPTokenCreatorTests
{
    private INonceStore _nonceStore;
    private ProofKeyConfiguration _keyConfiguration;
    private DPoPTokenCreator _dPoPTokenCreator;
    private JsonWebKey _jsonWebKey;

    [SetUp]
    public void SetUp()
    {
        _nonceStore = Substitute.For<INonceStore>();
        _jsonWebKey = new JsonWebKey
        {
            Alg = "RS256",
            E = "AQAB",
            Kty = "RSA",
            N = "test_n"
        };
        _keyConfiguration = new ProofKeyConfiguration(_jsonWebKey);
        _dPoPTokenCreator = new DPoPTokenCreator(_nonceStore, _keyConfiguration);
    }

    [Test]
    public async Task CreateSignedToken_CreatesValidToken_WithNonce()
    {
        // Arrange
        var method = HttpMethod.Get;
        var url = "https://example.com";
        var nonce = "test-nonce";
        _nonceStore.SetNonce(url, method.ToString(), nonce).Returns(Task.CompletedTask);

        // Act
        var token = await _dPoPTokenCreator.CreateSignedToken(method, url, nonce);

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
        _nonceStore.GetNonce(url, method.ToString()).Returns(Task.FromResult(generatedNonce));

        // Act
        var token = await _dPoPTokenCreator.CreateSignedToken(method, url);

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
        _nonceStore.SetNonce(url, method.ToString(), nonce).Returns(Task.CompletedTask);

        // Act
        var token = await _dPoPTokenCreator.CreateSignedToken(method, url, nonce);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.That(jwtToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Jti), Is.True);
        Assert.That(jwtToken.Claims.Any(c => c.Type == DPoPClaimNames.HttpMethod && c.Value == method.ToString().ToUpperInvariant()), Is.True);
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
        _nonceStore.SetNonce(url, method.ToString(), nonce).Returns(Task.CompletedTask);

        // Act
        var token = await _dPoPTokenCreator.CreateSignedToken(method, url, nonce);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var jwk = new JsonWebKey(jwtToken.Header[JwtHeaderParameterNames.Jwk].ToString());

        Assert.That(jwtToken.Header[JwtHeaderParameterNames.Typ], Is.EqualTo("dpop+jwt"));
        Assert.That(jwtToken.Header[JwtHeaderParameterNames.Jwk], Is.Not.Null);
        Assert.That(jwk.Alg, Is.EqualTo("PS256"));
    }
}
