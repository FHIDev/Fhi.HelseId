using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Api.Handlers
{
    /// <summary>
    /// At least one scope in the list of access token scopes must be present in the list of allowed scopes
    /// </summary>
    public class ApiMultiScopeHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdApiKonfigurasjon _configAuth;
        private readonly ILogger<ApiMultiScopeHandler> _logger;

        public ApiMultiScopeHandler(IHelseIdApiKonfigurasjon configAuth, ILogger<ApiMultiScopeHandler> logger)
        {
            _configAuth = configAuth;
            _logger = logger;
            logger.LogTrace("ApiMultiScopeHandler initialized. Security check enabled for requirement: {requirement}.", nameof(SecurityLevelOrApiRequirement));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            var clientId = context.User.FindFirst("client_id")?.Value ?? "???";
            var clientName = context.User.FindFirst("helseid://claims/client/client_name")?.Value ?? "???";
            _logger.LogInformation("ApiMultiScopeHandler: Validating, Request ClientId {clientId} ClientName {clientName}", clientId, clientName);
            var scopeClaims = context.User.FindAll("scope").Where(s => s.Value.StartsWith(_configAuth.ApiName)).ToList();
            foreach (var claim in scopeClaims)
            {
                _logger.LogTrace("Scope claim: {claim}.", claim.Value);
            }

            if (!scopeClaims.Any())
            {
                _logger.LogError("No scopes found, access denied.");
                return Task.CompletedTask;
            }

            var scopes = scopeClaims.Select(o => o.Value.Trim().ToLower());
            var allowedScopes = _configAuth.ApiScope.Split(",").Select(o => o.Trim().ToLower()).ToList();
            if (!allowedScopes.Any())
            {
                _logger.LogError("No scopes defined in configuration.");
                return Task.CompletedTask;
            }

            foreach (var allowedScope in allowedScopes)
            {
                _logger.LogTrace("Allowed scope: {allowedScope}.", allowedScope);
            }

            var matches = scopes.Intersect(allowedScopes);
            if (matches.Any())
            {
                _logger.LogTrace("Succeeded.");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogError("Missing or invalid scope {scopeClaims}, access denied.", string.Join(',', scopeClaims));
            }

            return Task.CompletedTask;
        }
    }
}
