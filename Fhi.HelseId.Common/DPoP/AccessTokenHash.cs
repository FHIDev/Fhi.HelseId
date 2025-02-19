using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Common.DPoP;

public static class AccessTokenHash
{
    /// <summary>
    /// Sha-256 hash of the access token. The value MUST be the result of a base64url encoding (as defined in Section 2 of [RFC7515]) the SHA-256 [SHS] hash of the ASCII encoding of the associated access token's value.
    /// </summary>
    /// <param name="accessToken">Access token to hash</param>
    /// <returns>Sha256-hash of the access token</returns>
    public static string Sha256(string accessToken)
    {
        using var encryptor = SHA256.Create();
        var input = Encoding.ASCII.GetBytes(accessToken);
        var sha256 = encryptor.ComputeHash(input);
        return Base64UrlEncoder.Encode(sha256);
    }
}
