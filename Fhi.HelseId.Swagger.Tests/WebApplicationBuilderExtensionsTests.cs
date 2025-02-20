using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Swagger.Tests;

public class WebApplicationBuilderExtensionsTests
{
    [TestCase("Prod")]
    [TestCase("Production")]
    public void SwaggerTokenServicesAreNotRegisteredForProdEnvironments(string environmentName)
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = environmentName;
        var swaggerHelseIdConfiguration = new SwaggerHelseIdConfiguration();

        // Act
        builder.AddHelseIdTestTokens(swaggerHelseIdConfiguration);

        // Assert
        Assert.That(builder.Services, Is.All.Not.Matches<ServiceDescriptor>(sd => sd.ServiceType == typeof(ITokenProxy)));
        Assert.That(builder.Services, Is.All.Not.Matches<ServiceDescriptor>(sd => sd.ServiceType == typeof(SwaggerHelseIdConfiguration)));
    }

    [TestCase("Test")]
    [TestCase("Stage")]
    [TestCase("Development")]
    public void SwaggerTokenServicesAreRegisteredForTestEnvironments(string environmentName)
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = environmentName;
        var swaggerHelseIdConfiguration = new SwaggerHelseIdConfiguration();

        // Act
        builder.AddHelseIdTestTokens(swaggerHelseIdConfiguration);

        // Assert
        Assert.That(builder.Services, Has.Some.Matches<ServiceDescriptor>(sd => sd.ServiceType == typeof(ITokenProxy)));
        Assert.That(builder.Services, Has.Some.Matches<ServiceDescriptor>(sd => sd.ServiceType == typeof(SwaggerHelseIdConfiguration)));
    }

    [Test]
    public void SwaggerTokenServicesAreRegisteredForSpecifiedEnvironments()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "SwaggerRequiredEnvironment";
        var swaggerHelseIdConfiguration = new SwaggerHelseIdConfiguration();

        // Act
        builder.AddHelseIdTestTokens(swaggerHelseIdConfiguration, ["SwaggerRequiredEnvironment"]);

        // Assert
        Assert.That(builder.Services, Has.Some.Matches<ServiceDescriptor>(sd => sd.ServiceType == typeof(ITokenProxy)));
        Assert.That(builder.Services, Has.Some.Matches<ServiceDescriptor>(sd => sd.ServiceType == typeof(SwaggerHelseIdConfiguration)));
    }

    [Test]
    public void SwaggerTokenServicesAreNotRegisteredForUnspecifiedEnvironments()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "RandomEnvironment";
        var swaggerHelseIdConfiguration = new SwaggerHelseIdConfiguration();

        // Act
        builder.AddHelseIdTestTokens(swaggerHelseIdConfiguration, ["SwaggerRequiredEnvironment"]);

        // Assert
        Assert.That(builder.Services, Is.All.Not.Matches<ServiceDescriptor>(sd => sd.ServiceType == typeof(ITokenProxy)));
        Assert.That(builder.Services, Is.All.Not.Matches<ServiceDescriptor>(sd => sd.ServiceType == typeof(SwaggerHelseIdConfiguration)));
    }
}
