using System.Security.Claims;
using System.Text.Encodings.Web;
using Fhi.TestFramework.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Fhi.TestFramework.AuthenticationSchemes.TestAuthenticationScheme
{
    /// <summary>
    /// Authentication handler to simulate loggedin user
    /// </summary>
    internal class TestAuthenticationHandler : SignInAuthenticationHandler<TestAuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!string.IsNullOrEmpty(Options.AccessToken))
            {
                var authProperties = new AuthenticationProperties();
                authProperties.Items["id_token"] = Options.IdToken;
                authProperties.Items["access_token"] = Options.AccessToken;
                authProperties.IsPersistent = true;

                IEnumerable<Claim> claims = CreateClaims(Options.AccessToken, Options.IdToken);
                var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
                var ticket = claimsIdentity.CreateAuthenticationTicket(Scheme.Name, authProperties);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            if (Options.UserClaims is not null)
            {
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(Options.UserClaims, "TestAuthentication")), Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("TestAuthenticationSchemeOptions has not set UserClaims or AccessToken not set"));
        }

        private static IEnumerable<Claim> CreateClaims(string accessToken, string? idToken)
        {
            var accessTokenJwt = new JsonWebToken(accessToken);
            var idTokenJwt = new JsonWebToken(idToken);
            return accessTokenJwt.Claims.Concat(idTokenJwt.Claims);
        }

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }
    }
}
