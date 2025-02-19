using System;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement
{
    public class AutomaticTokenManagementOptions
    {
        public string? Scheme { get; set; }
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
        public bool RevokeRefreshTokenOnSignout { get; set; } = false;

        /// <summary>
        /// Event callback that will be called for the cookie this automatic token management is applied to.
        /// </summary>
        public CookieAuthenticationEvents CookieEvents { get; set; } = new();
    }
}
