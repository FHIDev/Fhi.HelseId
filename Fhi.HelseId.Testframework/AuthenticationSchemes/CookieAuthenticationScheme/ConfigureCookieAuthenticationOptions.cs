using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Fhi.TestFramework.AuthenticationSchemes.CookieAuthenticationScheme
{
    public class ConfigureCookieAuthenticationOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        public void Configure(string? name, CookieAuthenticationOptions options)
        {
            Configure(options);
        }

        public void Configure(CookieAuthenticationOptions options)
        {
            options.CookieManager = new FakeCookieManager();
            options.TicketDataFormat = new FakeCookieTicketDataFormat();
        }
    }
}
