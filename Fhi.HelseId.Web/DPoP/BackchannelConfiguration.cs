using System.Net.Http;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace Fhi.HelseId.Web.DPoP;

public class BackchannelConfiguration(
    BackchannelHandler backchannelHandler)
    : IConfigureNamedOptions<OpenIdConnectOptions>
{
    public void Configure(string? name, OpenIdConnectOptions options)
    {
        if (name == HelseIdContext.Scheme)
        {
            // If there is already a configured handler, we must chain this.
            if (options.BackchannelHttpHandler == null)
            {
                backchannelHandler.InnerHandler = new HttpClientHandler();
            }
            else
            {
                backchannelHandler.InnerHandler = options.BackchannelHttpHandler;
            }

            options.BackchannelHttpHandler = backchannelHandler;
        }
    }

    public void Configure(OpenIdConnectOptions options)
    { }
}
