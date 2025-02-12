using System.Linq;
using Fhi.HelseId.Common.DPoP;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Api.DPoP;

// Source code originally from https://github.com/NorskHelsenett/HelseID.Samples/blob/d88f5ffdae47cd34975e1d597433e53995fdd935/Common/ApiDPoPValidation/DPoPExtensions.cs#L26
public static class DPoPExtensions
{
    private const string DPoPAuthorizationSchema = AuthorizationScheme.DPoP + " ";

    public static bool TryGetDPoPAccessToken(this HttpRequest request, out string token)
    {
        token = "";
        var authorization = request.Headers.Authorization.SingleOrDefault() ?? "";
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(DPoPAuthorizationSchema, System.StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        token = authorization.Substring(DPoPAuthorizationSchema.Length);
        return true;
    }

    public static bool TryGetDPoPProof(this HttpRequest request, out string dPopProof)
    {
        dPopProof = request.Headers[DPoPHttpHeaders.ProofHeaderName].SingleOrDefault() ?? "";
        return !string.IsNullOrEmpty(dPopProof);
    }

    public static string CreateThumbprint(this JsonWebKey jwk)
    {
        return Base64UrlEncoder.Encode(jwk.ComputeJwkThumbprint());
    }
}
