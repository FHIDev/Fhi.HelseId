using Fhi.HelseId.Api.DPoP;
using Fhi.HelseId.Api.ExtensionMethods;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Api
{
    public static class HelseIdOptionsExtensions
    {
        public static AuthenticationBuilder AddHelseIdJwtBearer(this AuthenticationBuilder authenticationBuilder,
            IHelseIdApiKonfigurasjon configAuth)
        {
            var builder = authenticationBuilder.AddJwtBearer(
                options =>
                {
                    options.Authority = configAuth.Authority;
                    options.Audience = configAuth.ApiName;
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.RefreshOnIssuerKeyNotFound = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireSignedTokens = true,
                        RequireAudience = true,
                        RequireExpirationTime = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidTypes = ["at+jwt", "JWT"]
                    };

                    if (configAuth.AllowDPoPTokens || configAuth.RequireDPoPTokens)
                    {
                        options.EnableDPoP(configAuth.RequireDPoPTokens);
                    }
                });

            if (!configAuth.RequireDPoPTokens)
            {
                builder.Services.AddHostedService<DPoPComplianceWarning>();
            }

            return builder;
        }
    }
}
