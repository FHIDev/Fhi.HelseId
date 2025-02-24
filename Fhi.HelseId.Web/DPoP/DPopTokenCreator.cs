using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Fhi.HelseId.Common.DPoP;
using Fhi.HelseId.Web.Services;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Web.DPoP;

public interface IDPoPTokenCreator
{
    Task<string> CreateSignedToken(HttpMethod method, string url, string? nonce = null, string? ath = null);
    Task<int> DoStuffAsync(HttpMethod methodm, string? param = null, string? param2 = null);
}

public class DPoPTokenCreator(
    INonceStore nonceStore,
    IHelseIdClientSecretHandler secretHandler) : IDPoPTokenCreator
{
    public async Task<string> CreateSignedToken(HttpMethod method, string url, string? nonce = null, string? ath = null)
    {
        var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti),
            new(DPoPClaimNames.HttpMethod, method.ToString().ToUpperInvariant()),
            new(DPoPClaimNames.HttpUrl, url),
            new(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64),
        };

        await AddNonceToClaims(nonce, claims, url, method);

        if (!string.IsNullOrEmpty(ath))
        {
            claims.Add(new Claim("ath", ath));
        }

        var jwk = secretHandler.GetSecurityKey().AsDPoPJwkSecret();
        var signingCredentials = new SigningCredentials(jwk, jwk.Alg);

        var jwtSecurityToken = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials);
        jwtSecurityToken.Header.Remove(JwtHeaderParameterNames.Typ);
        jwtSecurityToken.Header.Add(JwtHeaderParameterNames.Typ, "dpop+jwt");
        jwtSecurityToken.Header.Add(JwtHeaderParameterNames.Jwk, GetPublicKey(jwk));

        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return token;
    }

    private async Task AddNonceToClaims(string? nonce, List<Claim> claims, string url, HttpMethod method)
    {
        if (nonce == null)
        {
            nonce = await nonceStore.GetNonce(url, method.ToString());
        }
        else
        {
            await nonceStore.SetNonce(url, method.ToString(), nonce);
        }

        if (!string.IsNullOrEmpty(nonce))
        {
            claims.Add(new(JwtRegisteredClaimNames.Nonce, nonce));
        }
    }

    private static Dictionary<string, string> GetPublicKey(JsonWebKey jwk) =>
        new Dictionary<string, string>
        {
            { JsonWebKeyParameterNames.Alg, jwk.Alg },
            { JsonWebKeyParameterNames.E, jwk.E },
            { JsonWebKeyParameterNames.Kty, jwk.Kty },
            { JsonWebKeyParameterNames.N, jwk.N }
        }
        .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public Task<int> DoStuffAsync(HttpMethod methodm, string? param = null, string? param2 = null)
    {
        throw new NotImplementedException();
    }
}
