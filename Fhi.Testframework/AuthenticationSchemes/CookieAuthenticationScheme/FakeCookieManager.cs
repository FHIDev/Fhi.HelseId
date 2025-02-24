using Fhi.TestFramework.NHNTTT;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Fhi.TestFramework.AuthenticationSchemes.CookieAuthenticationScheme
{
    internal class FakeCookieManager : ICookieManager
    {
        public void AppendResponseCookie(HttpContext context, string key, string? value, CookieOptions options)
        {
            throw new NotImplementedException();
        }

        public void DeleteCookie(HttpContext context, string key, CookieOptions options)
        {
            throw new NotImplementedException();
        }

        public string GetRequestCookie(HttpContext context, string key)
        {
            var task = TTTService.GetHelseIdToken(TTTTokenRequests.DefaultAccessToken("fhi-api-access"));

            return task.Result;
        }
    }
}
