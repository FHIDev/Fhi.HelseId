using System.ComponentModel.DataAnnotations;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Configuration;

namespace Fhi.HelseId.Api
{
    public interface IHelseIdApiFeatures
    {
        public bool UseHttps { get; }
        public bool AuthUse { get; }
    }

    public interface IHelseIdApiKonfigurasjon : IAutentiseringkonfigurasjon
    {
        /// <summary>
        /// OIDC authority for HelseID
        /// </summary>
        [Required]
        string Authority { get; }

        /// <summary>
        /// Audience
        /// </summary>
        [Required]
        string ApiName { get; }

        bool UseHttps { get; }

        /// <summary>
        /// Allows DPoP-authorization headers on incoming API-calls in addition to Bearer tokens.
        /// </summary>
        bool AllowDPoPTokens { get; }

        /// <summary>
        /// Requires all incoming API-calls to use DPoP authorization.
        /// </summary>
        bool RequireDPoPTokens { get; }
    }

    public class HelseIdApiKonfigurasjon : HelseIdCommonKonfigurasjon, IHelseIdApiFeatures, IHelseIdApiKonfigurasjon
    {
        public string ApiName { get; set; } = "";

        public bool AllowDPoPTokens { get; set; } = true;
        public bool RequireDPoPTokens { get; set; }
    }
}
