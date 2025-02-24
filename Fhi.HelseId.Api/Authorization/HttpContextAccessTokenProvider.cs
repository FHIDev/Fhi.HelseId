using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Fhi.HelseId.Api.ExtensionMethods;
using Microsoft.AspNetCore.Http;

// Code based on https://github.com/HelseID/HelseID.Samples/tree/master/HelseId.Core.BFF.Sample, originally written by Magnus Bjerke Vik
namespace Fhi.HelseId.Api.Authorization
{
    public class HttpContextAccessTokenProvider : IAccessTokenProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private string? cachedAccessToken;

        /// <summary>
        /// Provides access tokens from the <see cref="HttpContext"/>, if available.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <remarks>This implementation caches the access token, assuming that its lifetime is associated with a single HttpContext, so that the access token does not change.</remarks>
        public HttpContextAccessTokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<JwtSecurityToken?> GetAccessTokenAsJwtSecurityToken()
        {
            var accessToken = await GetAccessToken();
            if (accessToken == null)
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);
            return jwt;
        }

        public async Task<string?> GetAccessToken()
        {
            return await GetAccessTokenFromHttpContext();
        }

        private async Task<string?> GetAccessTokenFromHttpContext()
        {
            if (cachedAccessToken == null)
            {
                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    cachedAccessToken = await httpContext.AccessToken();
                }
            }

            return cachedAccessToken;
        }
    }
}
