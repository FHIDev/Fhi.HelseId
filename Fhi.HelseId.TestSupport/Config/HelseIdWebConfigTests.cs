using Fhi.HelseId.Common.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class HelseIdWebConfigTests : HelseIdClientBased
    {
        protected HelseIdWebConfigTests(string file, bool test, AppSettingsUsage useOfAppsettings) : base(file, test, useOfAppsettings)
            => HelseIdWebKonfigurasjonUnderTest = Config
                .GetSection(nameof(Web.HelseIdWebKonfigurasjon))
                .Get<Web.HelseIdWebKonfigurasjon>()!;

        protected Web.HelseIdWebKonfigurasjon HelseIdWebKonfigurasjonUnderTest { get; }

        protected override HelseIdCommonKonfigurasjon HelseIdKonfigurasjonUnderTest => HelseIdWebKonfigurasjonUnderTest;

        protected override HelseIdClientKonfigurasjon HelseIdClientKonfigurasjon => HelseIdWebKonfigurasjonUnderTest;

        [Test]
        public void ThatAuthorityHasNoSuffix()
        {
            Guard();

            Assert.That(HelseIdWebKonfigurasjonUnderTest.Authority, Does.EndWith(".nhn.no/"),
                $"{ConfigFile}: A Webapp should use the authority url without any suffixes.");
        }

        protected override void Guard()
        {
            Assert.That(HelseIdWebKonfigurasjonUnderTest, Is.Not.Null, $"{ConfigFile}: No config section named 'HelseIdWebKonfigurasjon' found, or derived");
        }
    }
}
