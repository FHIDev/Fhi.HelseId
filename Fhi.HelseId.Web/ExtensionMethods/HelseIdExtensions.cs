using System;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.DPoP;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fhi.HelseId.Web.ExtensionMethods
{
    public static class HelseIdExtensions
    {
        public static void DefaultHelseIdOptions(this CookieAuthenticationOptions options,
            IHelseIdWebKonfigurasjon configAuth,
            IRedirectPagesKonfigurasjon redirectPagesKonfigurasjon)
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = configAuth.UseHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
            options.AccessDeniedPath = redirectPagesKonfigurasjon.Forbidden;
            // NOTE: options.Events must be set in AddAutomaticTokenManagement.
            // This is because it overrides the events set here.
        }

        public static void DefaultHelseIdOptions(this OpenIdConnectOptions options,
            IHelseIdWebKonfigurasjon configAuth,
            IRedirectPagesKonfigurasjon redirectPagesKonfigurasjon,
            IHelseIdSecretHandler secretHandler)
        {
            var acrValues = GetAcrValues(configAuth); // spesielt for id-porten, e.g. krever sikkerhetsnivå 4
            var hasAcrValues = !string.IsNullOrWhiteSpace(acrValues);
            options.Authority = configAuth.Authority;
            options.RequireHttpsMetadata = true;
            options.ClientId = configAuth.ClientId;
#if NET9_0
            options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Require;
#endif
            options.ResponseType = "code";
            options.TokenValidationParameters.ValidAudience = configAuth.ClientId;
            options.TokenValidationParameters.ValidTypes = ["at+jwt", "JWT"];
            options.CallbackPath = "/signin-callback";
            options.SignedOutCallbackPath = "/signout-callback";
            options.Scope.Clear();
            //options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            //options.NonceCookie.SameSite = SameSiteMode.Lax;

            if (configAuth.RequireHprNumber)
            {
                options.GetClaimsFromUserInfoEndpoint = true;
                options.ClaimActions.MapUniqueJsonKey(ClaimsPrincipalExtensions.HprDetails, ClaimsPrincipalExtensions.HprDetails);
            }

            foreach (var scope in configAuth.AllScopes)
            {
                options.Scope.Add(scope.Trim());
            }
            options.SaveTokens = true;

            options.Events.OnRedirectToIdentityProvider = ctx =>
            {
                //API requests should get a 401 status instead of being redirected to login
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.Headers["Location"] = ctx.ProtocolMessage.CreateAuthenticationRequestUrl();
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    ctx.HandleResponse();
                }

                if (ctx.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication && hasAcrValues)
                {
                    ctx.ProtocolMessage.AcrValues = acrValues;
                }

                if (configAuth.RewriteRedirectUriHttps)
                {
                    // Rewrite Redirect Uri to use https in case e.g. running from container
                    var builder = new UriBuilder(ctx.ProtocolMessage.RedirectUri)
                    {
                        Scheme = "https",
                        Port = -1
                    };
                    ctx.ProtocolMessage.RedirectUri = builder.ToString();
                }

                if (configAuth.UseDPoPTokens)
                {
                    var proofGenerator = ctx.HttpContext.RequestServices.GetRequiredService<IProofRedirector>();
                    proofGenerator.AttachThumbprint(ctx);
                }

                return Task.CompletedTask;
            };

            options.AccessDeniedPath = redirectPagesKonfigurasjon.Forbidden;

            if (configAuth.UseDPoPTokens)
            {
                options.ForwardDPoPContext();
            }

            secretHandler.AddSecretConfiguration(options);

            string GetAcrValues(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon)
            {
                return string.Join(' ', helseIdWebKonfigurasjon.SecurityLevels.Select(sl => $"Level{sl}"));
            }
        }


        /// <summary>
        /// Setter default helse-id opsjoner for automatisk token management,parameter for refresh tid i minutter
        /// </summary>
        /// <param name="options"></param>
        /// <param name="refreshBeforeExpirationTime">Tid i minutter</param>
        public static void DefaultHelseIdOptions(this AutomaticTokenManagementOptions options, double refreshBeforeExpirationTime)
        {
            options.RefreshBeforeExpiration = TimeSpan.FromMinutes(refreshBeforeExpirationTime);
            options.RevokeRefreshTokenOnSignout = true;
            options.Scheme = HelseIdContext.Scheme;

            options.CookieEvents.OnRedirectToAccessDenied = ctx =>
            {
                // API requests should get a 403 status instead of being redirected to access denied page
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.Headers["Location"] = ctx.RedirectUri;
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                }

                return Task.CompletedTask;
            };
        }
    }
}
