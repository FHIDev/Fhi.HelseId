using System;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.DPoP;
using Fhi.HelseId.Web.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fhi.HelseId.Web.OIDC
{
    public class OidcEvents : OpenIdConnectEvents
    {
        private readonly IHelseIdClientSecretHandler _secretHandler;
        private readonly IHelseIdWebKonfigurasjon _helseIdWebKonfigurasjon;

        public OidcEvents(IHelseIdClientSecretHandler secretHandler, IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon)
        {
            _secretHandler = secretHandler;
            _helseIdWebKonfigurasjon = helseIdWebKonfigurasjon;
        }

        public override async Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            if (context.TokenEndpointRequest == null)
            {
                throw new InvalidOperationException($"{nameof(context.TokenEndpointRequest)} cannot be null");
            }

            if (context.Options.ConfigurationManager is not null)
            {
                string clientAssertion = await GenerateClientAssertion(context.Options.ConfigurationManager);

                context.TokenEndpointRequest.ClientAssertionType = OidcConstants.ClientAssertionTypes.JwtBearer;
                context.TokenEndpointRequest.ClientAssertion = clientAssertion;
            }
        }

#if NET9_0
        public override async Task PushAuthorization(PushedAuthorizationContext context)
        {
            if (context.Options.ConfigurationManager is not null)
            {
                string clientAssertion = await GenerateClientAssertion(context.Options.ConfigurationManager);

                context.ProtocolMessage.ClientAssertionType = OidcConstants.ClientAssertionTypes.JwtBearer;
                context.ProtocolMessage.ClientAssertion = clientAssertion;
            }
        }
#endif

        public override Task RedirectToIdentityProvider(RedirectContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.Headers["Location"] = context.ProtocolMessage.CreateAuthenticationRequestUrl();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.HandleResponse();
            }

            var acrValues = GetAcrValues(_helseIdWebKonfigurasjon);

            if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication && !string.IsNullOrWhiteSpace(acrValues))
            {
                context.ProtocolMessage.AcrValues = acrValues;
            }

            if (_helseIdWebKonfigurasjon.RewriteRedirectUriHttps)
            {
                // Rewrite Redirect Uri to use https in case e.g. running from container
                var builder = new UriBuilder(context.ProtocolMessage.RedirectUri)
                {
                    Scheme = "https",
                    Port = -1
                };
                context.ProtocolMessage.RedirectUri = builder.ToString();
            }

            if (_helseIdWebKonfigurasjon.UseDPoPTokens)
            {
                var proofGenerator = context.HttpContext.RequestServices.GetRequiredService<IProofRedirector>();
                proofGenerator.AttachThumbprint(context);
            }

            return Task.CompletedTask;
        }

        public override Task MessageReceived(MessageReceivedContext context)
        {
            if (_helseIdWebKonfigurasjon.UseDPoPTokens)
            {
                // Simply forward the flag that this request is being done in context of DPoP
                // so that the backchannel handler knows we are talking DPoP.
                if (context.Properties?.Items.ContainsKey(DPoPContext.ContextKey) == true)
                {
                    context.HttpContext.Items[DPoPContext.ContextKey] = "true";
                }

                return Task.CompletedTask;
            }

            return base.MessageReceived(context);
        }

        private string GetAcrValues(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon)
        {
            return string.Join(' ', helseIdWebKonfigurasjon.SecurityLevels.Select(sl => $"Level{sl}"));
        }

        private async Task<string> GenerateClientAssertion(IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
        {
            var configuration = await configurationManager.GetConfigurationAsync(default);
            var jwkKey = _secretHandler.GetSecurityKey();
            var clientAssertion = ClientAssertion.Generate(_helseIdWebKonfigurasjon.ClientId, configuration.Issuer, jwkKey);
            return clientAssertion;
        }
    }
}
