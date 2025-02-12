using Fhi.HelseId.Common.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.TestSupport.Config;

public abstract class HelseIdClientBased : BaseConfigTests
{
    protected HelseIdClientBased(string configFile, bool test, AppSettingsUsage useOfAppsettings) : base(configFile, test, useOfAppsettings)
    {
    }

    protected abstract HelseIdClientKonfigurasjon HelseIdClientKonfigurasjon { get; }

    [Test]
    public void ThatClientIdIsSet()
    {
        Guard();
        Assert.That(HelseIdClientKonfigurasjon.ClientId.Length, Is.GreaterThan(0), $"{ConfigFile}: ClientId is not set");
        TestContext.Out.WriteLine($"ClientId is: {HelseIdClientKonfigurasjon.ClientId}");
    }

    [Test]
    public void ThatScopesDontExceedLimit()
    {
        const int maxScopeLengthInHelseId = 600;
        Guard();
        string scopes = string.Join(',', HelseIdClientKonfigurasjon.AllScopes);
        Assert.That(scopes.Length, Is.LessThanOrEqualTo(maxScopeLengthInHelseId), $"{ConfigFile}: Combined scopes have a maximum length of 600 - including separators, this is {scopes.Length}");
        TestContext.Out.WriteLine($"{ConfigFile}: Total length of scopes is {scopes.Length}");
    }

    [Test]
    public void ThatAuthUseIsTrue()
    {
        Guard();
        Assert.That(HelseIdClientKonfigurasjon.AuthUse, $"{ConfigFile} AuthUse should be true, but is false");
    }
}
