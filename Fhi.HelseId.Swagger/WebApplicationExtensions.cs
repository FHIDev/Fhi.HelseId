using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fhi.HelseId.Swagger;

public static class WebApplicationExtensions
{
    public static RouteHandlerBuilder UseHelseIdTestTokenEndpoint(this WebApplication app, string[]? enabledEnvironments = null)
    {
        var currentEnvironmentName = app.Environment.EnvironmentName;

        var isProduction = currentEnvironmentName.StartsWith("Prod", StringComparison.OrdinalIgnoreCase);

        if (enabledEnvironments?.Contains(currentEnvironmentName) ?? true && !isProduction)
        {
            return app.MapPost(".swagger-dev/token", async Task<IResult> ([FromForm] IFormCollection form, ITokenProxy tokenProxy) =>
            {
                var requestParameters = form.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.First());
                var jsonResult = await tokenProxy.RequestToken(requestParameters);

                return Results.Text(jsonResult, MediaTypeNames.Application.Json);
            })
            .DisableAntiforgery()
            .ExcludeFromDescription();
        }

        return new RouteHandlerBuilder([]);
    }
}
