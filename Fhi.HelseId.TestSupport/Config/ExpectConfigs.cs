using Fhi.HelseId.Common.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using HelseIdApiKonfigurasjon = Fhi.HelseId.Api.HelseIdApiKonfigurasjon;

namespace Fhi.HelseId.TestSupport.Config
{
    /// <summary>
    /// Setup as parametrized fixture with configuration filename and nameof expected configuration type
    /// Verifies that they are actually present and valid
    /// </summary>
    /// <typeparam name="T">The expected configuration type</typeparam>
    public abstract class ExpectConfigs<T> : SetupBaseConfigTests
        where T : HelseIdCommonKonfigurasjon
    {
        private readonly string configNameOf;
        private readonly IConfigurationSection section;
        private readonly T? config;

        protected ExpectConfigs(string file, string configNameOf, AppSettingsUsage useOfAppsettings) : base(file, useOfAppsettings)
        {
            this.configNameOf = configNameOf;
            section = Config.GetSection(configNameOf);
            config = section.Get<T>();
        }

        [Test]
        public void ThatSectionExistInConfigFile()
        {
            Assert.That(section.Exists, $"No config section named {configNameOf} found in {ConfigFile}");
        }

        [Test]
        public void ThatSectionIsExpectedConfiguration()
        {
            Assert.That(config, Is.Not.Null, $"Configuration for {configNameOf} in {ConfigFile} is not valid");
        }

        protected override void Guard()
        {
        }
    }

    /// <summary>
    /// Checks that the config file contains an API configuration
    /// Setup as parametrized fixture with configuration filename
    /// Verifies that they are actually present and valid
    /// Code example:
    /// [TestFixture("appsettings.json")]
    /// public MyCheckClass : ExpectHelseIdApiConfig
    /// {
    ///     public void MyCheckClass(string filename) : base(filename)
    ///     {}
    /// }
    /// Add as many TestFixture attributes on top as you have configuration files
    /// </summary>
    public abstract class ExpectHelseIdApiConfig : ExpectConfigs<HelseIdApiKonfigurasjon>
    {
        protected ExpectHelseIdApiConfig(string filename, AppSettingsUsage useOfAppsettings) : base(filename, nameof(HelseIdApiKonfigurasjon), useOfAppsettings)
        {
        }
    }
}
