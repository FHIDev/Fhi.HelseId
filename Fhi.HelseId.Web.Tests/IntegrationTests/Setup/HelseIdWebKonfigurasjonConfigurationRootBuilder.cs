using System.Text.Json;
using Fhi.TestFramework.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Fhi.HelseId.Web.IntegrationTests.Setup
{
    internal static class HelseIdWebKonfigurasjonConfigurationRootBuilder
    {
        internal static IConfigurationRoot CreateConfigurationRoot(this HelseIdWebKonfigurasjon configuration)
        {
            return CreateHelseIdWebConfiguration(
                configuration.Scopes,
                string.Join(" ", configuration.SecurityLevels),
                configuration.AuthUse,
                configuration.Authority,
                configuration.ClientId,
                configuration.RequireHprNumber,
                configuration.IncludeHprNumber,
                configuration.RequireValidHprAuthorization);
        }

        private static IConfigurationRoot CreateHelseIdWebConfiguration(
            string[] scopes,
            string securityLevel = "3", 
            bool authUse = true, 
            string authority = "https://helseid-sts.test.nhn.no/", 
            string clientId = "1224", 
            bool requireHprNumber = false,
            bool includeHprNumber = false,
            bool requireValidHprAuthorization = false)
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.AuthUse)}", authUse.ToString() },
                { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.Authority)}", authority },
                { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.ClientId)}", clientId },
                { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.SecurityLevels)}", securityLevel },
                { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.RequireHprNumber)}", requireHprNumber.ToString() },
                { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.IncludeHprNumber)}", includeHprNumber.ToString() },
                { $"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.RequireValidHprAuthorization)}", requireValidHprAuthorization.ToString() }

            };

            // Add scopes to the dictionary with unique keys
            for (int i = 0; i < scopes.Length; i++)
            {
                appsettingsConfig[$"{nameof(HelseIdWebKonfigurasjon)}:{nameof(HelseIdWebKonfigurasjon.Scopes)}:{i}"] = scopes[i];
            }

            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();
            return testConfiguration;
        }
    }
}
