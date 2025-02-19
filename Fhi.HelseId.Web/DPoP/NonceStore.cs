using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Fhi.HelseId.Web.DPoP;

public interface INonceStore
{
    Task<string> GetNonce(string url, string method);
    Task SetNonce(string url, string method, string nonce);
}

public class NonceStore(IDistributedCache store) : INonceStore
{
    private static readonly TimeSpan ExpirationTime = TimeSpan.FromHours(1);
    private const string StoreKey = "DPoPNonce-";
    private static Encoding NonceEncoding = Encoding.UTF8;

    public async Task<string> GetNonce(string url, string method)
    {
        var key = ToStoreKey(url, method);
        var storedNonce = await store.GetAsync(key);

        if (storedNonce != null)
        {
            return NonceEncoding.GetString(storedNonce);
        }

        return "";
    }

    public async Task SetNonce(string url, string method, string nonce)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.Add(ExpirationTime),
        };

        var key = ToStoreKey(url, method);
        var encoded = NonceEncoding.GetBytes(nonce);

        await store.SetAsync(key, encoded, options);
    }

    private static string ToStoreKey(string url, string method)
    {
        return StoreKey + url + method;
    }
}
