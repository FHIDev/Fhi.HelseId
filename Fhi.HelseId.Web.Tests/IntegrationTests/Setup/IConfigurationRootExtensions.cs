using System.Text.Json;
using Fhi.TestFramework.Extensions;
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.Web.IntegrationTests.Setup
{
    internal static class IConfigurationRootExtensions
    {
        internal static IConfigurationRoot CreateConfigurationRoot(this HelseIdWebKonfigurasjon configuration)
        {
            return CreateHelseIdWebConfiguration(
                string.Join(" ", configuration.SecurityLevels),
                configuration.AuthUse,
                configuration.Authority,
                configuration.ClientId,
                JsonSerializer.Serialize(configuration.Scopes));
        }

        private static IConfigurationRoot CreateHelseIdWebConfiguration(string securityLevel = "3", bool authUse = true, string authority = "https://helseid-sts.test.nhn.no/", string clientId = "1224", string scopes = "fhi:api-scope")
        {
            var appsettingsConfig = new Dictionary<string, string?>
                {
                    { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.AuthUse)}", authUse.ToString() },
                    { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.Authority)}",  authority },
                    { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.ClientId)}", clientId },
                    ////{ $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.Scopes)}", scopes },
                    { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.Scopes)}", "Aditional_scope" },
                    { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.SecurityLevels)}",  securityLevel }
                };
            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();
            return testConfiguration;
        }
    }
}
