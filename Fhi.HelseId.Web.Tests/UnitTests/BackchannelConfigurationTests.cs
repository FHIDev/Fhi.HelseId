using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.DPoP;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Web.UnitTests;

public class BackchannelConfigurationTests
{
    private BackchannelHandler _backchannelHandler = null!;
    private BackchannelConfiguration _backchannelConfiguration = null!;
    private OpenIdConnectOptions _options = null!;
    private IHttpContextAccessor _httpContextAccessor = null!;
    private IDPoPTokenCreator _tokenHelper = null!;

    [SetUp]
    public void SetUp()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _tokenHelper = Substitute.For<IDPoPTokenCreator>();
        _backchannelHandler = new BackchannelHandler(_httpContextAccessor, _tokenHelper);
        _backchannelConfiguration = new BackchannelConfiguration(_backchannelHandler);
        _options = new OpenIdConnectOptions();
    }

    [TearDown]
    public void DisposeHandler()
    {
        _backchannelHandler.Dispose();
    }

    [Test]
    public void Configure_Sets_BackchannelHttpHandler_When_Handler_Is_Null_And_Name_Is_HelseIdContextScheme()
    {
        // Arrange
        string name = HelseIdContext.Scheme;
        _options.BackchannelHttpHandler = null;

        // Act
        _backchannelConfiguration.Configure(name, _options);

        // Assert
        Assert.That(_backchannelHandler.InnerHandler is HttpClientHandler, Is.True);
        Assert.That(_options.BackchannelHttpHandler, Is.EqualTo(_backchannelHandler));
    }

    [Test]
    public void Configure_Chains_BackchannelHttpHandler_When_Already_Exists_And_Name_Is_HelseIdContextScheme()
    {
        // Arrange
        string name = HelseIdContext.Scheme;
        var existingHandler = new HttpClientHandler();
        _options.BackchannelHttpHandler = existingHandler;

        // Act
        _backchannelConfiguration.Configure(name, _options);

        // Assert
        Assert.That(_backchannelHandler.InnerHandler, Is.EqualTo(existingHandler));
        Assert.That(_options.BackchannelHttpHandler, Is.EqualTo(_backchannelHandler));
    }

    [Test]
    public void Configure_Does_Not_Change_Handler_When_Name_Is_Not_HelseIdContextScheme()
    {
        // Arrange
        string name = "SomeOtherScheme";
        var existingHandler = new HttpClientHandler();
        _options.BackchannelHttpHandler = existingHandler;

        // Act
        _backchannelConfiguration.Configure(name, _options);

        // Assert
        Assert.That(_options.BackchannelHttpHandler, Is.EqualTo(existingHandler));
        Assert.That(_backchannelHandler.InnerHandler, Is.Null);
    }

    [Test]
    public void Configure_Generic_Options_Does_Nothing()
    {
        // Act
        _backchannelConfiguration.Configure(_options);

        // Assert
        Assert.That(_backchannelHandler.InnerHandler, Is.Null);
        Assert.That(_options.BackchannelHttpHandler, Is.Null);
    }
}
