using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Fhi.HelseId.Web
{
    public class ConfigureAuthentication
    {
        public Action<CookieAuthenticationOptions>? ConfigureCookieAuthentication { get; set; }
        public Action<OpenIdConnectOptions>? ConfigureOpenIdConnect { get; set; }
    }
}
