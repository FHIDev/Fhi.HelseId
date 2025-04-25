using Fhi.HelseId.Common.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using HelseIdApiKonfigurasjon = Fhi.HelseId.Api.HelseIdApiKonfigurasjon;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class HelseIdApiConfigTests : BaseConfigTests
    {
        protected HelseIdApiConfigTests(string file, bool test, AppSettingsUsage useOfAppsettings) : base(file, test, useOfAppsettings)
            => HelseIdApiKonfigurasjonUnderTest = Config
                .GetSection(nameof(HelseIdApiKonfigurasjon))
                .Get<HelseIdApiKonfigurasjon>()!;

        protected HelseIdApiKonfigurasjon HelseIdApiKonfigurasjonUnderTest { get; }

        protected override HelseIdCommonKonfigurasjon HelseIdKonfigurasjonUnderTest => HelseIdApiKonfigurasjonUnderTest;

        [Test]
        public void ThatAuthorityHasNoSuffix()
        {
            Guard();

            Assert.That(HelseIdApiKonfigurasjonUnderTest.Authority, Does.EndWith(".nhn.no/"),
                $"{ConfigFile}: An API should use the authority url without any suffixes.");
        }

        protected sealed override void Guard()
        {
            Assert.That(HelseIdApiKonfigurasjonUnderTest, Is.Not.Null, $"{ConfigFile}: No config section named 'HelseIdApiKonfigurasjon' found, or derived");
        }
    }
}
