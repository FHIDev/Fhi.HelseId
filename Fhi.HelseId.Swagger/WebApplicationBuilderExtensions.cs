using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Swagger;

public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="swaggerAuthConfig"></param>
    /// <param name="enabledEnvironments">Optional list of environments which the Swagger services will be registered for. If not specified, it will be registered for every environment except for production.</param>
    /// <returns></returns>
    public static WebApplicationBuilder AddHelseIdTestTokens(this WebApplicationBuilder builder, SwaggerHelseIdConfiguration swaggerAuthConfig, string[]? enabledEnvironments = null)
    {
        var currentEnvironmentName = builder.Environment.EnvironmentName;

        var isProduction = currentEnvironmentName.StartsWith("Prod", StringComparison.OrdinalIgnoreCase);

        if (enabledEnvironments?.Contains(currentEnvironmentName) ?? true && !isProduction)
        {
            builder.Services.AddSingleton(swaggerAuthConfig);
            builder.Services.AddSingleton<ITokenProxy, TokenProxy>();
        }

        return builder;
    }
}
