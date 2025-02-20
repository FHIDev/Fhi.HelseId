using System.Threading.Tasks;
using Fhi.HelseId.Common.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace Fhi.HelseId.Web.Handlers
{
    public class NoAuthorizationHandler : AuthorizationHandler<NoAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NoAuthorizationRequirement requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
