﻿using Fhi.HelseId.Web.ExtensionMethods;
using NUnit.Framework;

namespace Fhi.HelseId.Tests
{
    public class HelseIdWebKonfigurasjonTests : BaseConfigTests
    {
        [SetUp]
        public void Init() => base.Init();


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
                Assert.That(sut.UseHprNumber, Is.False, "4");
                Assert.That(sut.AllScopes, Has.Count.GreaterThan(6), "5");
                Assert.That(sut.Whitelist, Is.Not.Null);
                Assert.That(sut.Whitelist, Is.Empty);
                Assert.That(sut.Apis, Is.Not.Null);
                Assert.That(sut.Apis, Has.Length.EqualTo(2));
                Assert.That(sut.NoAuthenticationUser, Is.Not.Null);
                Assert.That(sut.NoAuthenticationUser.Claims, Has.Count.EqualTo(3));
            });
        }
    }

    
}