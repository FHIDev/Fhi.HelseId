using Microsoft.Extensions.Configuration;

namespace Fhi.TestFramework.Extensions
{
    public static class IConfigurationRootExtensions
    {
        public static IConfigurationRoot BuildInMemoryConfiguration(this Dictionary<string, string?> appsettingsConfig)
        {
            return new ConfigurationBuilder()
            .AddInMemoryCollection(appsettingsConfig)
            .Build();
        }

    }
}
