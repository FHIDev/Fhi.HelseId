using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Web.DPoP;

public static class JsonWebKeyExtensions
{
    /// <summary>
    /// Creates a copy of JsonWebKey since it is mutable and
    /// we only want to set the "Alg"-property for DPoP if it is not set.
    /// </summary>
    public static JsonWebKey AsDPoPJwkSecret(this JsonWebKey key)
    {
        var json = JsonSerializer.Serialize(key);
        var clonedKey = JsonSerializer.Deserialize<JsonWebKey>(json)!;
        clonedKey.Alg ??= "PS256";

        return clonedKey;
    }
}
