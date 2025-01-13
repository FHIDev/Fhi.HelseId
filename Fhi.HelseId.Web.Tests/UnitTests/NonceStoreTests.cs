using System.Text;
using Fhi.HelseId.Web.DPoP;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Web.UnitTests;

public class NonceStoreTests
{
    private IDistributedCache? _cache;
    private NonceStore? _nonceStore;

    [SetUp]
    public void SetUp()
    {
        _cache = Substitute.For<IDistributedCache>();
        _nonceStore = new NonceStore(_cache);
    }

    [Test]
    public async Task GetNonce_ReturnsStoredNonce_WhenNonceExists()
    {
        // Arrange
        var url = "https://example.com";
        var method = "GET";
        var expectedNonce = "stored-nonce";
        var key = "DPoPNonce-" + url + method;
        _cache!.GetAsync(key).Returns(Task.FromResult<byte[]?>(Encoding.UTF8.GetBytes(expectedNonce)));

        // Act
        var result = await _nonceStore!.GetNonce(url, method);

        // Assert
        Assert.That(result, Is.EqualTo(expectedNonce));
    }

    [Test]
    public async Task GetNonce_ReturnsEmptyString_WhenNonceDoesNotExist()
    {
        // Arrange
        var url = "https://example.com";
        var method = "GET";
        var key = "DPoPNonce-" + url + method;
        _cache!.GetAsync(key).Returns(Task.FromResult<byte[]?>(null));

        // Act
        var result = await _nonceStore!.GetNonce(url, method);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task SetNonce_SavesNonceWithCorrectKeyAndExpiration()
    {
        // Arrange
        var url = "https://example.com";
        var method = "POST";
        var nonce = "new-nonce";
        var key = "DPoPNonce-" + url + method;
        var encodedNonce = Encoding.UTF8.GetBytes(nonce);

        // Act
        await _nonceStore!.SetNonce(url, method, nonce);

        // Assert
        await _cache!.Received(1).SetAsync(key, Arg.Is<byte[]>(data => data.SequenceEqual(encodedNonce)), Arg.Is<DistributedCacheEntryOptions>(options =>
            options.AbsoluteExpiration.HasValue &&
            options.AbsoluteExpiration.Value.Subtract(DateTimeOffset.UtcNow).Minutes > 50));
    }
}