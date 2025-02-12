using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class SetupBaseConfigTests
    {
        /// <summary>
        /// AppSettingsIsProd,  // appsettings.prod exist and is merged with appsettings.json
        /// AppSettingsIsBaseOnly, // No seperate prod exist, appsettings.json is production, but when anything else is specified, merge is to happen
        /// AppSettingsIsTestWhenDev, // When developing, appsettings is development,  require seperate prod
        /// AppSettingsIsExplicit // Use only the specified appsettings.
        /// </summary>
        public enum AppSettingsUsage
        {
            AppSettingsIsProd,  // appsettings.prod exist and is merged with appsettings.json
            AppSettingsIsBaseOnly, // No seperate prod exist, appsettings.json is production, but when anything else is specified, merge is to happen
            AppSettingsIsTestWhenDev, // When developing, appsettings is development,  require seperate prod
            AppSettingsIsExplicit // Use only the specified appsettings.
        }

        protected string ConfigFile { get; }

        protected bool ConfigIsAppsettings => ConfigFile.Equals("appsettings.json", System.StringComparison.InvariantCultureIgnoreCase);
        protected SetupBaseConfigTests(string configFile, AppSettingsUsage appSettingsUsage)
        {
            ConfigFile = configFile;
            UseOfAppsettings = appSettingsUsage;
            Config = GetIConfigurationRoot(TestContext.CurrentContext.TestDirectory);
        }

        protected IConfigurationRoot Config;
        public AppSettingsUsage UseOfAppsettings { get; }

        protected abstract void Guard();

        private IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            var c = new ConfigurationBuilder()
                .SetBasePath(outputPath);
            if (!ConfigIsAppsettings && ShouldAddAppsettingsJson())
            {
                c.AddJsonFile(Path.Combine(outputPath, "appsettings.json"));
            }

            c.AddJsonFile(ConfigFile, optional: true);
            return c.Build();
        }

        private bool ShouldAddAppsettingsJson()
        {
            var result = UseOfAppsettings != AppSettingsUsage.AppSettingsIsExplicit;
            return result;
        }
    }
}
