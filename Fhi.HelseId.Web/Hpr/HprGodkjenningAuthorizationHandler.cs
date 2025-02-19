using System.Threading.Tasks;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Hpr
{
    public class HprGodkjenningAuthorizationHandler : AuthorizationHandler<HprGodkjenningAuthorizationRequirement>
    {
        private readonly IHprService _hprService;
        private readonly IWhitelist _whitelist;

        private ILogger<HprGodkjenningAuthorizationHandler> Logger { get; }
        public HprGodkjenningAuthorizationHandler(IHprService hprService, IGodkjenteHprKategoriListe godkjenninger, IWhitelist whitelist, ILogger<HprGodkjenningAuthorizationHandler> logger)
        {
            _whitelist = whitelist;
            _hprService = hprService;
            _hprService.LeggTilGodkjenteHelsepersonellkategorier(godkjenninger);
            Logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HprGodkjenningAuthorizationRequirement requirement)
        {
            var currentUser = context.User;
            var userlogName = currentUser.Name().ObfuscateName();
            Logger.LogTrace("HprGodkjenningAuthorizationHandler: Checking {Name}", userlogName);

            // TODO: remove this. It is unnecessary as should be handled by authorize filter or policy returning 401 and not 403.
            if (!currentUser.Identity?.IsAuthenticated ?? false)
            {
                Logger.LogWarning("HprGodkjenningAuthorizationHandler: Bruker {UserlogName} er ikke autentisiert", userlogName);
                context.Fail();
                return Task.CompletedTask;
            }

            var hprNummer = currentUser.HprNumber();
            if (hprNummer == null)
            {
                Logger.LogInformation("HprGodkjenningAuthorizationHandler: Bruker {UserlogName} har ikke hprnummer.", userlogName);
                SjekkWhitelist();
                return Task.CompletedTask;
            }

            var erGodkjent = _hprService.SjekkGodkjenning();
            if (erGodkjent)
            {
                Logger.LogTrace("HprGodkjenningAuthorizationHandler: {Name} autentisert", userlogName);
                context.Succeed(requirement);
            }
            else
            {
                Logger.LogInformation("HprGodkjenningAuthorizationHandler: Bruker {UserlogName} er ikke godkjent.", userlogName);
                SjekkWhitelist();
            }

            return Task.CompletedTask;

            void SjekkWhitelist()
            {
                if (_whitelist.IsWhite(currentUser.PidPseudonym() ?? ""))
                {
                    Logger.LogInformation("HprGodkjenningAuthorizationHandler: Bruker {UserlogName} er whitelisted.", userlogName);
                    context.Succeed(requirement);
                    return;
                }

                context.Fail();
            }
        }
    }
}
