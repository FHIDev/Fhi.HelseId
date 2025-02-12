using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Middleware
{
    /// <summary>
    /// Middleware that prevents unauthorized access to paths (by default all)
    /// </summary>
    public class ProtectPaths
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ProtectPaths> _logger;
        private readonly string _policyName;
        private readonly List<PathString> _excludedPaths;
        private readonly string _accessDeniedPath;

        public ProtectPaths(RequestDelegate next, ProtectPathsOptions options, ILogger<ProtectPaths> logger)
        {
            _next = next;
            _logger = logger;
            _policyName = options.Policy;
            _excludedPaths = options.Exclusions ?? [];
            _accessDeniedPath = options.AccessDeniedPath;
        }

        public async Task Invoke(HttpContext httpContext, IAuthorizationService authorizationService)
        {
            var path = httpContext.Request.Path;
            _logger.LogTrace($"ProtectedPaths: Checking path: {path}");

            /*
            * This check is important to ensure that incoming HTTP-calls are authorized for static files.
            * If the path to the static resource is not on the exclude list, it indicates that the user
            * does not have access to that resource without a valid auth token and the associated rights.
            * A side effect of this is that all calls to controllers will be authorized twice due to the
            * AuthorizeAsync call, which is invoked both through this middleware and as a policy
            * configured on the controllers themselves.
             */
            if (!_excludedPaths.Any(path.StartsWithSegments))
            {
                var endpoint = httpContext.GetEndpoint();
                var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>();

                if (allowAnonymous != null)
                {
                    await _next(httpContext);
                    return;
                }

                if (httpContext.User.Identity is not { IsAuthenticated: true })
                {
                    var redirectUri = httpContext.Request.GetEncodedPathAndQuery();
                    await httpContext.ChallengeAsync(new AuthenticationProperties { RedirectUri = redirectUri });
                    _logger.LogTrace("ProtectedPaths: User is not authenticated, ChallengeAsync called.");
                    return;
                }

                var authorizationResult = await authorizationService.AuthorizeAsync(httpContext.User, null, _policyName);
                if (!authorizationResult.Succeeded)
                {
                    _logger.LogTrace($"ProtectedPaths: Auth failed");
                    httpContext.Response.Redirect(_accessDeniedPath);
                    return;
                }
            }

            await _next(httpContext);
        }
    }

    public class ProtectPathsOptions
    {
        public ProtectPathsOptions(string policy, string accessDeniedPath)
        {
            Policy = policy;
            AccessDeniedPath = accessDeniedPath;
        }

        /// <summary>
        /// Policy required to access paths
        /// </summary>
        public string Policy { get; }

        /// <summary>
        /// Paths that don't require authentication
        /// </summary>
        public List<PathString>? Exclusions { get; set; }

        /// <summary>
        /// Path redirected to if user is not authorized to access paths
        /// </summary>
        public string AccessDeniedPath { get; }
    }

    public static class ProtectPathsExtensions
    {
        public static IApplicationBuilder UseProtectPaths(this IApplicationBuilder builder, ProtectPathsOptions options)
        {
            return builder.UseMiddleware<ProtectPaths>(options);
        }
    }
}
