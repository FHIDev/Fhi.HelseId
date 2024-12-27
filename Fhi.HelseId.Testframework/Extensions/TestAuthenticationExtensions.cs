using System.Security.Claims;
using Fhi.TestFramework.AuthenticationSchemes.TestAuthenticationScheme;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.TestFramework.Extensions
{
    public static class TestAuthenticationExtensions
    {
        /// <summary>
        /// Adds authentication scheme used for simulate login
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="authenticationScheme"></param>
        /// <param name="accessToken"></param>
        /// <param name="idToken"></param>
        /// <returns></returns>
        internal static AuthenticationBuilder AddFakeAuthenticationScheme(this AuthenticationBuilder builder, string authenticationScheme, string? accessToken = null, string? idToken = null)
        {
            return builder.AddFakeAuthenticationScheme(authenticationScheme, options =>
            {
                options.AccessToken = accessToken;
                options.IdToken = idToken;
            });
        }

        internal static AuthenticationBuilder AddFakeAuthenticationScheme(this AuthenticationBuilder builder, string authenticationScheme, IEnumerable<Claim> claims)
        {
            return builder.AddFakeAuthenticationScheme(authenticationScheme, options =>
            {
                options.UserClaims = claims;
            });
        }

        internal static AuthenticationBuilder AddFakeAuthenticationScheme(this AuthenticationBuilder builder, string authenticationScheme, Action<TestAuthenticationSchemeOptions>? options)
        {
            builder.Services.AddOptions<TestAuthenticationSchemeOptions>(authenticationScheme);
            return builder.AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>(authenticationScheme, authenticationScheme, options);
        }
    }
}
