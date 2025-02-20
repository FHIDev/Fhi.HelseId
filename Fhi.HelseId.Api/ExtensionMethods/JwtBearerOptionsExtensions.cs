using System.Threading.Tasks;
using Fhi.HelseId.Api.DPoP;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Api.ExtensionMethods;

public static class JwtBearerOptionsExtensions
{
    public static void EnableDPoP(this JwtBearerOptions options, bool requireDPoP)
    {
        options.Events ??= new JwtBearerEvents();
        options.Events.OnMessageReceived = context =>
        {
            var tokenHandler = context.HttpContext.RequestServices.GetRequiredService<IJwtBearerDPoPTokenHandler>();
            tokenHandler.ValidateAuthorizationHeader(context, requireDPoP);

            return Task.CompletedTask;
        };

        options.Events.OnTokenValidated = async tokenValidatedContext =>
        {
            var tokenHandler = tokenValidatedContext.HttpContext.RequestServices.GetRequiredService<IJwtBearerDPoPTokenHandler>();
            await tokenHandler.ValidateDPoPProof(tokenValidatedContext, requireDPoP);
        };
    }
}
