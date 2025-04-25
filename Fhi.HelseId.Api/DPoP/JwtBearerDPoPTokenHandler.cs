using System.Security.Claims;
using Fhi.HelseId.Common.DPoP;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Api.DPoP;

public interface IJwtBearerDPoPTokenHandler
{
    void ValidateAuthorizationHeader(MessageReceivedContext context, bool requireDPoPTokens = true);
    Task ValidateDPoPProof(TokenValidatedContext tokenValidatedContext, bool requireDPoPTokens = true);
}

public class JwtBearerDPoPTokenHandler(
    IDPoPProofValidator dPoPProofValidator, ILogger<JwtBearerDPoPTokenHandler> logger) : IJwtBearerDPoPTokenHandler
{
    public void ValidateAuthorizationHeader(MessageReceivedContext context, bool requireDPoPTokens = true)
    {
        var requestHasDPoPAccessToken = context.Request.TryGetDPoPAccessToken(out var dPopToken);

        if (requestHasDPoPAccessToken)
        {
            context.Token = dPopToken;
        }

        if (!requestHasDPoPAccessToken && requireDPoPTokens)
        {
            context.Fail("Request has no DPoP token, which is required");
        }
    }

    public async Task ValidateDPoPProof(TokenValidatedContext tokenValidatedContext, bool requireDPoPTokens = true)
    {
        var request = tokenValidatedContext.HttpContext.Request;

        // Get the access token:
        var requestIsDPoP = request.TryGetDPoPAccessToken(out var accessToken);

        if (!requestIsDPoP)
        {
            if (requireDPoPTokens)
            {
                tokenValidatedContext.Fail("Request has no DPoP token, which is required");
            }

            return;
        }

        if (!request.TryGetDPoPProof(out var dPopProof))
        {
            tokenValidatedContext.Fail("There must be exactly one DPoP proof header present");
            return;
        }

        // Get the cnf claim from the access token:
        var cnfClaimValue = tokenValidatedContext.Principal!.FindFirstValue(DPoPClaimNames.Confirmation);

        var data = new DPoPProofValidationData(request, dPopProof!, accessToken!, cnfClaimValue);

        var validationResult = await dPoPProofValidator.Validate(data);
        if (validationResult.IsError)
        {
            logger.LogError("Dpop validation failed {ErrorDescription}", validationResult.ErrorDescription);
            tokenValidatedContext.Fail(validationResult.ErrorDescription!);
        }
    }
}
