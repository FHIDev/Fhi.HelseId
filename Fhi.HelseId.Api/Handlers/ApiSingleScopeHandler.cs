using System;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Api.Handlers
{
    /// <summary>
    /// This scope handler expects a single scope in the configuration, and should not be set if there are multiple scopes
    /// </summary>
    public class ApiSingleScopeHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdApiKonfigurasjon _configAuth;
        private readonly ILogger<ApiSingleScopeHandler> _logger;

        public ApiSingleScopeHandler(IHelseIdApiKonfigurasjon configAuth, ILogger<ApiSingleScopeHandler> logger)
        {
            _configAuth = configAuth;
            _logger = logger;
            logger.LogTrace("ApiSingleScopeHandler initialized. Security level check enabled for requirement: {requirement}.", nameof(SecurityLevelOrApiRequirement));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            var clientId = context.User.FindFirst("client_id")?.Value ?? "???";
            var clientName = context.User.FindFirst("helseid://claims/client/client_name")?.Value ?? "???";
            _logger.LogInformation("Validating, Request ClientId: {clientId}, ClientName: {clientName}.", clientId, clientName);
            var scopeClaims = context.User.FindAll("scope").ToList();
            if (scopeClaims.Count == 0)
            {
                _logger.LogError("No scopes found.");
                return Task.CompletedTask;
            }

            if (scopeClaims.Any(c => StringComparer.InvariantCultureIgnoreCase.Equals(c.Value, _configAuth.ApiScope)))
            {
                _logger.LogTrace("Succeeded.");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogError("Missing or invalid scope '{scopeClaims}', access denied.", string.Join(',', scopeClaims));
            }

            return Task.CompletedTask;
        }
    }
}
