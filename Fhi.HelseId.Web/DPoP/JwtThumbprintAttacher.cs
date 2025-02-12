using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Web.DPoP;

public interface IProofRedirector
{
    void AttachThumbprint(RedirectContext ctx);
}

public class JwtThumbprintAttacher(IHelseIdSecretHandler secretHandler) : IProofRedirector
{
    public void AttachThumbprint(RedirectContext ctx)
    {
        var dpopSecret = secretHandler.GetSecurityKey().AsDPoPJwkSecret();
        var jkt = Base64UrlEncoder.Encode(dpopSecret.ComputeJwkThumbprint());

        ctx.Properties.Items[DPoPContext.ContextKey] = "true";
        ctx.ProtocolMessage.Parameters["dpop_jkt"] = jkt;
    }
}
