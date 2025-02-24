using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.Web.UnitTests
{
    public class HelseIdWebKonfigurasjonTests
    {
        [SetUp]
        public void Init()
        {
            Config = GetIConfigurationRoot(TestContext.CurrentContext.TestDirectory + "/UnitTests", "appsettings.test.json");
        }

        [Test]
        public void HelseIdWebConfigurationTest()
        {
            var sut = Config?.GetWebKonfigurasjon();
            Assert.That(sut, Is.Not.Null, "Can't load appsettings.test.json");
            Assert.Multiple(() =>
            {
                Assert.That(sut!.Authority, Does.Contain("helseid-sts"), "1");
                Assert.That(sut.AuthUse, Is.True, "2");
                Assert.That(sut.UseHttps, Is.True, "3");
                Assert.That(sut.RequireHprNumber, Is.False, "4");
                Assert.That(sut.AllScopes.ToList(), Has.Count.EqualTo(7), "5");
                Assert.That(sut.Whitelist, Is.Not.Null);
                Assert.That(sut.Whitelist, Is.Empty);
                Assert.That(sut.Apis, Is.Not.Null);
                Assert.That(sut.Apis, Has.Length.EqualTo(2));
                Assert.That(sut.NoAuthenticationUser, Is.Not.Null);
                Assert.That(sut.NoAuthenticationUser.Claims, Has.Count.EqualTo(3));
            });
        }

        [Test]
        public void HelseIdWebConfigurationTestWithHprNumber()
        {
            var sut = Config?.GetWebKonfigurasjon();
            Assert.That(sut, Is.Not.Null, "Can't load appsettings.test.json");
            sut!.RequireHprNumber = true;
            Assert.Multiple(() =>
            {
                Assert.That(sut!.Authority, Does.Contain("helseid-sts"), "1");
                Assert.That(sut.AuthUse, Is.True, "2");
                Assert.That(sut.UseHttps, Is.True, "3");
                Assert.That(sut.RequireHprNumber, Is.True, "4");
                Assert.That(sut.AllScopes.ToList(), Has.Count.EqualTo(8), "5");
                Assert.That(sut.Whitelist, Is.Not.Null);
                Assert.That(sut.Whitelist, Is.Empty);
                Assert.That(sut.Apis, Is.Not.Null);
                Assert.That(sut.Apis, Has.Length.EqualTo(2));
                Assert.That(sut.NoAuthenticationUser, Is.Not.Null);
                Assert.That(sut.NoAuthenticationUser.Claims, Has.Count.EqualTo(3));
            });
        }

        protected IConfigurationRoot? Config { get; private set; }

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath, string filename)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile(filename, optional: true)
                .Build();
        }
    }
}
