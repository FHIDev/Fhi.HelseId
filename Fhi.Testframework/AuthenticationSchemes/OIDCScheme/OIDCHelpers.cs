using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

// TODO: This is under construction. Will be needed when testing OIDC scheme
namespace Fhi.TestFramework.AuthenticationSchemes.OIDCScheme
{
    internal class TestStateDataFormat : ISecureDataFormat<AuthenticationProperties>
    {
        public string Protect(AuthenticationProperties data)
        {
            return "protected_state";
        }

        public string Protect(AuthenticationProperties data, string? purpose)
        {
            throw new NotImplementedException();
        }

        public AuthenticationProperties Unprotect(string? protectedText)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string?>()
                {
                    { ".xsrf", "correlationId" },
                    { OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, "redirect_uri" },
                    { "testkey", "testvalue" }
                })
            {
                RedirectUri = "http://testhost/redirect"
            };
            return properties;
        }

        public AuthenticationProperties Unprotect(string? protectedText, string? purpose)
        {
            throw new NotImplementedException();
        }
    }

    internal class TestTokenHandler : TokenHandler
    {
        public override Task<TokenValidationResult> ValidateTokenAsync(string token, TokenValidationParameters validationParameters)
        {
            var jwt = new JwtSecurityToken();
            return Task.FromResult(new TokenValidationResult()
            {
                SecurityToken = new JsonWebToken(jwt.EncodedHeader + "." + jwt.EncodedPayload + "."),
                ClaimsIdentity = new ClaimsIdentity("customAuthType"),
                IsValid = true
            });
        }

        public override SecurityToken ReadToken(string token)
        {
            return new JsonWebToken(token);
        }
    }
}
