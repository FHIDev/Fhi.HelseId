using Fhi.HelseId.Api.Services;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Identity;
using NSubstitute;

namespace Fhi.HelseId.Api.UnitTests
{
    public class WhitelistTests
    {
        private Whitelist? whitelist;
        private ICurrentUser? user;

        [SetUp]
        public void Init()
        {
            whitelist = new Whitelist
                { new White { Name = "Per", PidPseudonym = "1234" }, new White { Name = "Arne", PidPseudonym = "5678" } };
            user = Substitute.For<ICurrentUser>();
            user.PidPseudonym.Returns("5678");
        }

        [Test]
        public void ThatItFindsCorrectItemByUser()
        {
            Assert.Multiple(() =>
            {
                Assert.That(whitelist!.IsWhite(user!.PidPseudonym!));
                Assert.That(whitelist.NameOf(user!.PidPseudonym!), Is.EqualTo("Arne"));
            });
        }

        [Test]
        public void ThatItHandlesNonpresent()
        {
            user!.PidPseudonym.Returns("9999");
            Assert.Multiple(() =>
            {
                Assert.That(whitelist!.IsWhite(user!.PidPseudonym!), Is.False);
                Assert.That(whitelist.NameOf(user!.PidPseudonym!), Is.EqualTo(""));
            });
        }

        [Test]
        public void ThatItHandlesInvalid()
        {
            Assert.That(whitelist!.IsWhite(""), Is.False);
        }
    }
}
