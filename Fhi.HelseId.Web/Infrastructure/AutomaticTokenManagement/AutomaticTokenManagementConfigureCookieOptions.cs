using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement
{
    public class AutomaticTokenManagementConfigureCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly AuthenticationScheme _scheme;

        public AutomaticTokenManagementConfigureCookieOptions(IAuthenticationSchemeProvider provider, ILogger<AutomaticTokenManagementConfigureCookieOptions> logger)
        {
            var scheme = provider.GetDefaultSignInSchemeAsync().GetAwaiter().GetResult();
            if (scheme == null)
            {
                throw new InvalidOperationException("Field 'scheme' cannot be null");
            }

            _scheme = scheme;
        }

        public void Configure(CookieAuthenticationOptions options)
        { }

        public void Configure(string? name, CookieAuthenticationOptions options)
        {
            if (name == _scheme.Name)
            {
                options.EventsType = typeof(AutomaticTokenManagementCookieEvents);
            }
        }
    }
}
