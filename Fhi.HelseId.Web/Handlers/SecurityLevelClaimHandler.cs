using System;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Handlers
{
    public class SecurityLevelClaimHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdWebKonfigurasjon configAuth;
        private readonly ILogger<SecurityLevelClaimHandler> logger;

        public SecurityLevelClaimHandler(IHelseIdWebKonfigurasjon configAuth, ILogger<SecurityLevelClaimHandler> logger)
        {
            this.configAuth = configAuth;
            this.logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            logger.LogTrace("SecurityLevelClaimHandler: Validating");

            if (!context.User.Identity?.IsAuthenticated ?? false)
            {
                logger.LogInformation("SecurityLevelClaimHandler: User is not authenticated, access denied");
                return Task.CompletedTask;
            }

            var securityLevelClaim = context.User.FindFirst(c => c.Type.ToLowerInvariant() == IdentityClaims.SecurityLevel);

            if (securityLevelClaim != null)
            {
                if (configAuth.SecurityLevels.Any(sl => string.Equals(sl, securityLevelClaim.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    logger.LogTrace("SecurityLevelClaimHandler: Succeeded");
                    context.Succeed(requirement);
                }
                else
                {
                    logger.LogError("SecurityLevelClaimHandler: Invalid security level claim '{securityLevelClaim}', access denied.", securityLevelClaim.Value);
                }
            }
            else
            {
                logger.LogError("SecurityLevelClaimHandler: No security level claim found, access denied");
            }

            return Task.CompletedTask;
        }
    }
}
