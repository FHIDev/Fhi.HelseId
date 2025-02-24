using System.Security.Cryptography;
using Fhi.HelseId.Web.DPoP;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Web.UnitTests;

internal class JwtThumbprintAttacherTests
{
    private JwtThumbprintAttacher? _thumbprintAttacher;
    private RedirectContext? _redirectContext;
    private IHelseIdClientSecretHandler? _secretHandler;

    private byte[]? _expectedThumbprint;

    [SetUp]
    public void SetUp()
    {
        var rsaKey = new RsaSecurityKey(RSA.Create(2048));
        var proofKey = JsonWebKeyConverter.ConvertFromSecurityKey(rsaKey);
        _expectedThumbprint = proofKey.ComputeJwkThumbprint();

        var authScheme = new AuthenticationScheme("test", "test", typeof(IAuthenticationHandler));
        _secretHandler = Substitute.For<IHelseIdClientSecretHandler>();
        _secretHandler.GetSecurityKey().Returns(proofKey);
        _thumbprintAttacher = new JwtThumbprintAttacher(_secretHandler);
        _redirectContext = new RedirectContext(
            Substitute.For<HttpContext>(),
            authScheme, new OpenIdConnectOptions(),
            new AuthenticationProperties());
        _redirectContext.ProtocolMessage = new OpenIdConnectMessage();
    }

    [Test]
    public void AttachThumbprint_AddsJwkThumbprintToProtocolMessage()
    {
        // Act
        _thumbprintAttacher!.AttachThumbprint(_redirectContext!);

        // Assert
        var expectedThumbprint = Base64UrlEncoder.Encode(_expectedThumbprint);
        Assert.That(_redirectContext!.ProtocolMessage.Parameters["dpop_jkt"], Is.EqualTo(expectedThumbprint));
    }

    [Test]
    public void AttachThumbprint_SetsContextKeyInProperties()
    {
        // Act
        _thumbprintAttacher!.AttachThumbprint(_redirectContext!);

        // Assert
        Assert.That(_redirectContext!.Properties.Items[DPoPContext.ContextKey], Is.EqualTo("true"));
    }
}
