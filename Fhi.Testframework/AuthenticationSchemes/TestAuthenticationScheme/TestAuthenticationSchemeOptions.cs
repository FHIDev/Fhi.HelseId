using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Fhi.TestFramework.AuthenticationSchemes.TestAuthenticationScheme
{
    internal class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Use this if testing with an access_token
        /// Reads the access_token and add access_token claims to authentication ticket
        /// </summary>
        public string? AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Use this if testing with id_token. It reads id_token and applys claims to authentication ticket. When claim types exists in both id_token and access_token
        /// it will use claim from access_token
        /// </summary>
        public string? IdToken { get; set; } = string.Empty;

        /// <summary>
        /// Use this when not testing with a token
        /// </summary>
        public IEnumerable<Claim>? UserClaims { get; set; }
    }
}




