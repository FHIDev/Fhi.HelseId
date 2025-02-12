using System.Collections.Generic;
using System.Linq;

namespace Fhi.HelseId.Common.Configuration
{
    public interface IHelseIdClientKonfigurasjon : IAutentiseringkonfigurasjon
    {
        bool UseHttps { get; set; }
        bool RewriteRedirectUriHttps { get; set; }
        string Authority { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string[] Scopes { get; set; }
        bool Debug { get; set; }
        IEnumerable<string> AllScopes { get; }
        string JsonWebKeySecret { get; set; }
        string RsaKeySecret { get; set; }
        Whitelist Whitelist { get; set; }
    }

    public abstract class HelseIdClientKonfigurasjon : HelseIdCommonKonfigurasjon, IHelseIdClientKonfigurasjon
    {
        public bool RewriteRedirectUriHttps { get; set; } = false;
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string[] Scopes { get; set; } = [];
        public bool Debug { get; set; } = false;
        public IEnumerable<string> AllScopes => FixedScopes.Concat(Scopes).Distinct();
        public string JsonWebKeySecret { get; set; } = "";
        public string RsaKeySecret { get; set; } = "";
        public Whitelist Whitelist { get; set; } = new();

        protected List<string>? AllTheScopes { get; private set; }
        protected virtual IEnumerable<string> FixedScopes => new List<string>
        {
            "offline_access"
        };
    }
}
