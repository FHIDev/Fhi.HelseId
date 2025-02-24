using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Fhi.HelseId.Api.DPoP;

public interface IReplayCache
{
    Task AddAsync(string purpose, string handle, DateTimeOffset expiration);

    Task<bool> ExistsAsync(string purpose, string handle);
}

public class InMemoryReplayCache(IDistributedCache cache) : IReplayCache
{
    private const string Prefix = "ReplayCache-";

    public async Task AddAsync(string purpose, string handle, DateTimeOffset expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        };

        await cache.SetAsync(Prefix + purpose + handle, new byte[] { }, options);
    }

    public async Task<bool> ExistsAsync(string purpose, string handle)
    {
        return await cache.GetAsync(Prefix + purpose + handle) != null;
    }
}
