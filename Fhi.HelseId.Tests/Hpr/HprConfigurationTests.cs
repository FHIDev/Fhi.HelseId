using System;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Tests.Builders;
using Fhi.HelseId.Tests.FakeWebApplication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Fhi.HelseId.Tests.Hpr
{
    [TestFixture]
    public class HprConfigurationTests
    {
        [TestCase(true, true, true, 4)]
        [TestCase(true, false, true, 4)]
        [TestCase(true, true, false, 3)]
        [TestCase(true, false, false, 2)]
        [TestCase(false, false, false, 2)]
        public void HprFeatureFlags_ShouldAddCorrectServices(
            bool includeHprNumber,
            bool requireHprNumber,
            bool requireValidHprAuthorization,
            int expectedNumberOfIAuthorizationHandlers)
        {
            var config = HelseIdWebConfigurasjonBuilder.Create
                .Default()
                .WithIncludeHprNumber(includeHprNumber)
                .WithRequireHprNumber(requireHprNumber)
                .WithRequireValidHprAuthorization(requireValidHprAuthorization);

            var factory = new HelseIdWebApplication<Program>(config);

            var authHandler = factory.Services.GetServices<IAuthorizationHandler>();

            Assert.That(authHandler.Count, Is.EqualTo(expectedNumberOfIAuthorizationHandlers));
        }

        [Test]
        public async Task HprFeatureFlagsAllSetToFalse_ShouldSetCorrectPolicies()
        {
            var config = HelseIdWebConfigurasjonBuilder.Create.Default();

            var factory = new HelseIdWebApplication<Program>(config);

            var policyAuthenticated = await GetPolicy(factory, Policies.Authenticated);
            var policyHidOrPid = await GetPolicy(factory, Policies.HidOrApi);
            var policyHprNumber = await GetPolicy(factory, Policies.HprNummer);
            var policyGodkjentHprKategoriPolicy = await GetPolicy(factory, Policies.GodkjentHprKategoriPolicy);

            Assert.Multiple(() =>
            {
                Assert.That(policyAuthenticated, Is.Null);
                Assert.That(policyHidOrPid, Is.Not.Null);
                Assert.That(policyHprNumber, Is.Null);
                Assert.That(policyGodkjentHprKategoriPolicy, Is.Null);
            });
        }

        [Test]
        public async Task HprFeatureFlagsIncludeHprNumberSetToTrue_ShouldSetCorrectPolicies()
        {
            var config = HelseIdWebConfigurasjonBuilder.Create
                .Default()
                .WithIncludeHprNumber(true);

            var factory = new HelseIdWebApplication<Program>(config);

            var policyAuthenticated = await GetPolicy(factory, Policies.Authenticated);
            var policyHidOrPid = await GetPolicy(factory, Policies.HidOrApi);
            var policyHprNumber = await GetPolicy(factory, Policies.HprNummer);
            var policyGodkjentHprKategoriPolicy = await GetPolicy(factory, Policies.GodkjentHprKategoriPolicy);

            Assert.Multiple(() =>
            {
                Assert.That(policyAuthenticated, Is.Null);
                Assert.That(policyHidOrPid, Is.Not.Null);
                Assert.That(policyHprNumber, Is.Null);
                Assert.That(policyGodkjentHprKategoriPolicy, Is.Null);
            });
        }

        [Test]
        public async Task HprFeatureFlagsIncludeRequireHprNumberSetToTrue_ShouldSetCorrectPolicies()
        {
            var config = HelseIdWebConfigurasjonBuilder.Create
                .Default()
                .WithRequireHprNumber(true);

            var factory = new HelseIdWebApplication<Program>(config);

            var policyAuthenticated = await GetPolicy(factory, Policies.Authenticated);
            var policyHidOrPid = await GetPolicy(factory, Policies.HidOrApi);
            var policyHprNumber = await GetPolicy(factory, Policies.HprNummer);
            var policyGodkjentHprKategoriPolicy = await GetPolicy(factory, Policies.GodkjentHprKategoriPolicy);

            Assert.Multiple(() =>
            {
                Assert.That(policyAuthenticated, Is.Null);
                Assert.That(policyHidOrPid, Is.Not.Null);
                Assert.That(policyHprNumber, Is.Not.Null);
                Assert.That(policyGodkjentHprKategoriPolicy, Is.Null);
            });
        }

        [Test]
        public async Task HprFeatureFlagsIncludeRequireValidHprAuthorizationSetToTrue_ShouldSetCorrectPolicies()
        {
            var config = HelseIdWebConfigurasjonBuilder.Create
                .Default()
                .WithRequireValidHprAuthorization(true);

            var factory = new HelseIdWebApplication<Program>(config);

            var policyAuthenticated = await GetPolicy(factory, Policies.Authenticated);
            var policyHidOrPid = await GetPolicy(factory, Policies.HidOrApi);
            var policyHprNumber = await GetPolicy(factory, Policies.HprNummer);
            var policyGodkjentHprKategoriPolicy = await GetPolicy(factory, Policies.GodkjentHprKategoriPolicy);

            Assert.Multiple(() =>
            {
                Assert.That(policyAuthenticated, Is.Null);
                Assert.That(policyHidOrPid, Is.Not.Null);
                Assert.That(policyHprNumber, Is.Not.Null);
                Assert.That(policyGodkjentHprKategoriPolicy, Is.Not.Null);
            });
        }

        private async Task<AuthorizationPolicy?> GetPolicy(HelseIdWebApplication<Program> factory, string policy)
            => await factory.Services.GetRequiredService<IAuthorizationPolicyProvider>().GetPolicyAsync(policy);
    }
}