using System.Collections.Generic;
using Fhi.HelseId.Web;

namespace Fhi.HelseId.Tests.Builders
{
    public class HelseIdWebConfigurasjonBuilder
    {
        private Dictionary<string, string?> _config = new Dictionary<string, string?>();

        public static HelseIdWebConfigurasjonBuilder Create
        {
            get { return new HelseIdWebConfigurasjonBuilder(); }
        }

        public HelseIdWebConfigurasjonBuilder Default()
        {
            _config.Add($"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.AuthUse)}", "true");

            return this;
        }

        public HelseIdWebConfigurasjonBuilder WithIncludeHprNumber(bool value)
        {
            _config.Add($"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.IncludeHprNumber)}", $"{value}");

            return this;
        }

        public HelseIdWebConfigurasjonBuilder WithRequireHprNumber(bool value)
        {
            _config.Add($"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.RequireHprNumber)}", $"{value}");

            return this;
        }

        public HelseIdWebConfigurasjonBuilder WithRequireValidHprAuthorization(bool value)
        {
            _config.Add($"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.RequireValidHprAuthorization)}", $"{value}");

            return this;
        }

        public static implicit operator Dictionary<string, string?>(HelseIdWebConfigurasjonBuilder builder)
            => builder._config;
    }
}