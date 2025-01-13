using System.Security.Claims;
using Fhi.TestFramework.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Fhi.TestFramework.AuthenticationSchemes.CookieAuthenticationScheme
{
    internal class FakeCookieTicketDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        public string Protect(AuthenticationTicket data)
        {
            throw new NotImplementedException();
        }

        public string Protect(AuthenticationTicket data, string? purpose)
        {
            throw new NotImplementedException();
        }

        public AuthenticationTicket? Unprotect(string? protectedText)
        {
            var jwt = new JsonWebToken(protectedText);
            var claimsIdentity = new ClaimsIdentity(jwt.Claims, "scheme");
            AuthenticationTicket ticket = claimsIdentity.CreateAuthenticationTicket("scheme", null);

            return ticket;
        }

        public AuthenticationTicket? Unprotect(string? protectedText, string? purpose)
        {
            return Unprotect(protectedText);
        }
    }
}
