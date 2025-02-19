using System.Threading.Tasks;
using Fhi.HelseId.Web.DPoP;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Fhi.HelseId.Web.ExtensionMethods;

public static class OidcOptionsExtensions
{
    public static void ForwardDPoPContext(this OpenIdConnectOptions options)
    {
        options.Events.OnMessageReceived = context =>
        {
            // Simply forward the flag that this request is being done in context of DPoP
            // so that the backchannel handler knows we are talking DPoP.
            if (context.Properties?.Items.ContainsKey(DPoPContext.ContextKey) == true)
            {
                context.HttpContext.Items[DPoPContext.ContextKey] = "true";
            }

            return Task.CompletedTask;
        };
    }
}
