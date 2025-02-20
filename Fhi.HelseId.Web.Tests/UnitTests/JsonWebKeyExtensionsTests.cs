using System.Security.Cryptography;
using Fhi.HelseId.Web.DPoP;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;

namespace Fhi.HelseId.Web.UnitTests;

public class JsonWebKeyExtensionsTests
{
    [Test]
    public void KeyShouldBeCloned()
    {
        // Arrange
        var rsaKey = new RsaSecurityKey(RSA.Create(2048));
        var proofKey = JsonWebKeyConverter.ConvertFromSecurityKey(rsaKey);

        // Act
        var key = proofKey.AsDPoPJwkSecret();
        key.Kty = "test";

        // Assert
        Assert.That(key.Kty, Is.Not.EqualTo(proofKey.Kty));
    }

    [Test]
    public void AlgIsSetToPS256ByDefault()
    {
        // Arrange
        var rsaKey = new RsaSecurityKey(RSA.Create(2048));
        var proofKey = JsonWebKeyConverter.ConvertFromSecurityKey(rsaKey);
        proofKey.Alg = null;

        // Act
        var key = proofKey.AsDPoPJwkSecret();

        // Assert
        Assert.That(key.Alg, Is.EqualTo("PS256"));
    }
}
