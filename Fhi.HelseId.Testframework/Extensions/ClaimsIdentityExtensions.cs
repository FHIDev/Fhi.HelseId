using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Fhi.TestFramework.Extensions
{
    internal static class ClaimsIdentityExtensions
    {
        internal static AuthenticationTicket CreateAuthenticationTicket(this ClaimsIdentity identity, string scheme, AuthenticationProperties? properties = null)
        {
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), properties, scheme);
            return ticket;
        }
    }
}
