using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fhi.AuthControllers
{
    [Route("Account")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private RedirectPagesKonfigurasjon RedirectConfig { get; }
        private HelseIdWebKonfigurasjon Config { get; }

        private readonly ILogger<AccountController> logger;
        public AccountController(IOptions<HelseIdWebKonfigurasjon> options, ILogger<AccountController> logger)
        {
            this.logger = logger;
            Config = options.Value;
            RedirectConfig = Config.RedirectPagesKonfigurasjon;
        }

        [AllowAnonymous]
        [HttpGet("Login")]
        public async Task Login()
        {
            if (Config.AuthUse)
            {
                logger.LogTrace("Account: Logging in");
                await HttpContext.ChallengeAsync(
                    HelseIdContext.Scheme,
                    new AuthenticationProperties
                    {
                        RedirectUri = Config.DevelopmentRedirectUri ?? "/"
                    });
            }
            else
            {
                logger.LogTrace("Account: Not using authentication, just directing you");
                HttpContext.Response.Redirect("/");
            }
        }

        [AllowAnonymous]
        [HttpGet("Logout")]
        public async Task Logout()
        {
            logger.LogTrace("Account: Logging out");
            if (Config.AuthUse)
            {
                await HttpContext.SignOutAsync(HelseIdContext.Scheme, new AuthenticationProperties
                {
                    RedirectUri = RedirectConfig.LoggedOut,
                });
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            else
            {
                logger.LogTrace("Account: Not using authentication, just directing you");
                HttpContext.Response.Redirect("/");
            }
        }

        [AllowAnonymous]
        [HttpGet("Ping")]
        public ActionResult<string> Ping()
        {
            return Ok("Pong");
        }
    }
}
