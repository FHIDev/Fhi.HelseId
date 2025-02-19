using System;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Web.ExtensionMethods;

public class HttpClientBuilder
{
    private readonly WebApplicationBuilder builder;
    private readonly HelseIdWebKonfigurasjon webConfig;

    public HttpClientBuilder(WebApplicationBuilder builder)
    {
        this.builder = builder;
        webConfig = builder.Configuration.GetWebKonfigurasjon() ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon));
    }

    /// <summary>
    /// Adds all apis defined in the configuration to the service collection
    /// </summary>
    /// <param name="extra">Optional function which will be added for each api</param>
    /// <returns>WebApplicationBuilder</returns>
    public WebApplicationBuilder AddApisUsingHttpClient(Func<IServiceCollection, IApiOutgoingKonfigurasjon, IServiceCollection>? extra = null)
    {
        if (webConfig.AuthUse)
        {
            foreach (var api in webConfig.Apis)
            {
                builder.Services.AddHttpClient(api.Name, configureClient: client =>
                    {
                        client.BaseAddress = api.Uri;
                        client.Timeout = TimeSpan.FromMinutes(api.Timeout);
                    })
                    .AddHttpMessageHandler<AuthHeaderHandler>();
                extra?.Invoke(builder.Services, api);
            }
        }
        else
        {
            foreach (var api in webConfig.Apis)
            {
                builder.Services.AddHttpClient(api.Name, client =>
                {
                    client.BaseAddress = api.Uri;
                    client.Timeout = TimeSpan.FromMinutes(api.Timeout);
                });
                extra?.Invoke(builder.Services, api);
            }
        }

        return builder;
    }
}

public static class Extensions
{
    /// <summary>
    /// Adds all apis defined in the configuration to the service collection
    /// Retrieve them in the named services using IHttpClientFactory
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="extra">Optional function which will be added for each api</param>
    /// <returns>WebApplicationBuilder</returns>
    public static WebApplicationBuilder AddApisUsingHttpClient(this WebApplicationBuilder builder, Func<IServiceCollection, IApiOutgoingKonfigurasjon, IServiceCollection>? extra = null)
        => new HttpClientBuilder(builder).AddApisUsingHttpClient(extra);
}
