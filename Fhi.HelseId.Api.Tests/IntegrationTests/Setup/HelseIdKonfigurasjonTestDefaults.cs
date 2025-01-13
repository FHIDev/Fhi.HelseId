using Fhi.HelseId.Api;

namespace Fhi.HelseId.Integration.Tests.HelseId.Api.Setup
{
    internal class HelseIdApiKonfigurasjonBuilder
    {
        internal static HelseIdApiKonfigurasjon Create
        {
            get
            {
                return new HelseIdApiKonfigurasjon();
            }
        }
    }

    internal static class HelseIdApiKonfigurasjonExtensions
    {
        internal static HelseIdApiKonfigurasjon DefaultValues(this HelseIdApiKonfigurasjon config, string audience = "fhi:api", string allowedScopes = "fhi:api/scope")
        {
            config.Authority = "https://helseid-sts.test.nhn.no";
            config.AuthUse = true;
            config.UseHttps = true;
            config.ApiName = audience;
            config.ApiScope = allowedScopes;
            return config;
        }

        internal static HelseIdApiKonfigurasjon WithRequireDPoPTokens(this HelseIdApiKonfigurasjon config, bool requireDPoPToken)
        {
            config.RequireDPoPTokens = requireDPoPToken;
            return config;
        }

        internal static HelseIdApiKonfigurasjon WithAllowDPoPTokens(this HelseIdApiKonfigurasjon config, bool allowDpopTokens)
        {
            config.AllowDPoPTokens = allowDpopTokens;
            return config;
        }

        internal static HelseIdApiKonfigurasjon WithAllowedScopes(this HelseIdApiKonfigurasjon config, string allowedScopes)
        {
            config.ApiScope = allowedScopes;
            return config;
        }
    }
}
