using System;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fhi.HelseId.Web.OIDC
{
    internal class OidcEvents : OpenIdConnectEvents
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
        private async Task<string> GenerateClientAssertion(IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
        {
            var configuration = await configurationManager.GetConfigurationAsync(default);
            var jwkKey = _secretHandler.GetSecurityKey();
            var clientAssertion = ClientAssertion.Generate(_helseIdWebKonfigurasjon.ClientId, configuration.Issuer, jwkKey);
            return clientAssertion;
        }
    }
}
