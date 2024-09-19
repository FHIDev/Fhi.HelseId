﻿
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
        string Authority { get;  }

        /// <summary>
        /// Name of the API provided by this application
        /// </summary>
        [Required]
        string ApiName { get;  }

        /// <summary>
        /// Scope for access to the API by a person
        /// </summary>
        [Required]
        string ApiScope { get;  }

        bool UseHttps { get;  }
        bool RequireContextIdentity { get; }
        bool AllowDPoPTokens { get; }
        bool RequireDPoPTokens { get; }
    }

    public class HelseIdApiKonfigurasjon : HelseIdCommonKonfigurasjon, IHelseIdApiFeatures, IHelseIdApiKonfigurasjon
    {
        public string ApiName { get; set; } = "";
        public string ApiScope { get; set; } = "";

        public bool RequireContextIdentity { get; set; } = false;
        public bool AllowDPoPTokens { get; set; }
        public bool RequireDPoPTokens { get; set; }
    }
}
