using Microsoft.AspNetCore.Authorization;

namespace Fhi.HelseId.Web.Hpr
{
    /// <summary>
    /// Anvendes for å sjekke helsepersonnelts godkjente katogori
    /// </summary>
    public class HprGodkjenningAuthorizationRequirement : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Anvendes for å sjekke HprNummer i HelseId
    /// </summary>
    public class HprAuthorizationRequirement : IAuthorizationRequirement
    {
    }
}
