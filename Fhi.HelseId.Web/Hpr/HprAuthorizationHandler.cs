using System.Threading.Tasks;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Hpr
{
    /// <summary>
    /// Handler for å sjekke HprNummer i HelseId. Slås på med UseHprNumber
    /// </summary>
    public class HprAuthorizationHandler : AuthorizationHandler<HprAuthorizationRequirement>
    {
        private readonly IWhitelist whitelist;

        private ILogger Logger { get; }
        public HprAuthorizationHandler(IWhitelist whitelist, ILogger<HprAuthorizationHandler> logger)
        {
            this.whitelist = whitelist;
            Logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HprAuthorizationRequirement requirement)
        {
            var currentUser = context.User;
            var userlogName = currentUser.Name().ObfuscateName();
            Logger.LogTrace("HprAuthorizationHandler: Checking {Name} with {PidPs}", userlogName, currentUser.PidPseudonym());
            if (currentUser.HprNumber() == null && !whitelist.IsWhite(currentUser?.PidPseudonym() ?? ""))
            {
                Logger.LogWarning("HprAuthorizationHandler: Failed. No HprNumber");
                context.Fail(new AuthorizationFailureReason(this, "Require hpr number"));
            }
            else
            {
                Logger.LogTrace("HprAuthorizationHandler: Succeeded");
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
