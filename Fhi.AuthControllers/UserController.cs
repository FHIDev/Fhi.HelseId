using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fhi.AuthControllers
{
    [Route("User")]
    [ApiController]
    [AllowAnonymous]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly ICurrentUser currentUser;

        public UserController(ILogger<UserController> logger, ICurrentUser currentUser)
        {
            this.logger = logger;
            this.currentUser = currentUser;
        }

        [HttpGet]
        public ActionResult<ICurrentUser> GetUser()
        {
            return Ok(currentUser);
        }
    }
}
