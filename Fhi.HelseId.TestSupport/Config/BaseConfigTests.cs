using Fhi.HelseId.Common.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class BaseConfigTests : SetupBaseConfigTests
    {
        protected bool IsTest { get; }

        protected BaseConfigTests(string configFile, bool test, AppSettingsUsage useOfAppsettings) : base(configFile, useOfAppsettings)
        {
            IsTest = test;
        }

        protected abstract HelseIdCommonKonfigurasjon HelseIdKonfigurasjonUnderTest { get; }

        [Test]
        public void AuthorityConfigurationTest()
        {
            const string testAuthorityUrl = "https://helseid-sts.test.nhn.no/";
            const string authorityUrl = "https://helseid-sts.nhn.no/";
            Guard();
            var authority = IsTest ? testAuthorityUrl : authorityUrl;
            Assert.That(HelseIdKonfigurasjonUnderTest.Authority, Does.StartWith(authority), $"Wrong authority found: {HelseIdKonfigurasjonUnderTest.Authority} in {ConfigFile}. Only possible with {testAuthorityUrl} for test, and {authorityUrl} for production");
        }
    }
}
