using Fhi.HelseId.Api.DPoP;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace Fhi.HelseId.Api.UnitTests;

public class InMemoryReplayCacheTests
{
    private InMemoryReplayCache _replayCache = null!;
    private IDistributedCache _distributedCacheMock = null!;

    private const string Purpose = "test-purpose";
    private const string Handle = "test-handle";
    private const string CacheKey = "ReplayCache-" + Purpose + Handle;

    [SetUp]
    public void SetUp()
    {
        _distributedCacheMock = Substitute.For<IDistributedCache>();
        _replayCache = new InMemoryReplayCache(_distributedCacheMock);
    }

    [Test]
    public async Task AddAsync_CallsSetAsyncOnCache_WithCorrectKeyAndExpiration()
    {
        // Arrange
        var expiration = DateTimeOffset.UtcNow.AddMinutes(5);
        var expectedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        };

        // Act
        await _replayCache.AddAsync(Purpose, Handle, expiration);

        // Assert
        await _distributedCacheMock.Received(1)
            .SetAsync(CacheKey, Arg.Is<byte[]>(b => b.Length == 0), Arg.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpiration == expiration));
    }

    [Test]
    public async Task ExistsAsync_ReturnsTrue_WhenCacheKeyExists()
    {
        // Arrange
        _distributedCacheMock.GetAsync(CacheKey).Returns(new byte[] { });

        // Act
        var result = await _replayCache.ExistsAsync(Purpose, Handle);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsAsync_ReturnsFalse_WhenCacheKeyDoesNotExist()
    {
        // Arrange
        _distributedCacheMock.GetAsync(CacheKey).Returns((byte[]?)null);

        // Act
        var result = await _replayCache.ExistsAsync(Purpose, Handle);

        // Assert
        Assert.That(result, Is.False);
    }
}
