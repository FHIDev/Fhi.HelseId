using System;
using System.Globalization;
using Fhi.HelseId.Common.Constants;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fhi.HelseId.Web.Infrastructure
{
    public static class CookieExtensions
    {
        public static bool UpdateAccessToken(this CookieValidatePrincipalContext ctx, string accessToken)
        {
            bool updated = ctx.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, accessToken);
            return updated;
        }

        public static bool UpdateRefreshToken(this CookieValidatePrincipalContext ctx, string refreshToken)
        {
            bool updated = ctx.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, refreshToken);
            return updated;
        }

        public static DateTimeOffset UpdateExpiresAt(this CookieValidatePrincipalContext ctx, DateTimeOffset expiresAt)
        {
            var newExpiresAt = expiresAt;
            ctx.Properties.UpdateTokenValue(OAuthConstants.ExpiresAt, newExpiresAt.ToString("o", CultureInfo.InvariantCulture));
            return newExpiresAt;
        }

        public static DateTimeOffset UpdateTokens(this CookieValidatePrincipalContext ctx, OidcToken tokenResponse)
        {
            ctx.UpdateAccessToken(tokenResponse.AccessToken);
            ctx.UpdateRefreshToken(tokenResponse.RefreshToken);
            var newExpiresAt = ctx.UpdateExpiresAt(tokenResponse.ExpiresAt);
            return newExpiresAt;
        }
    }
}
