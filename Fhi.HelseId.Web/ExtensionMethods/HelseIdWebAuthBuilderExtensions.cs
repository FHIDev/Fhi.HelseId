using System.Collections.Generic;
using Fhi.HelseId.Common.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Web.ExtensionMethods;

public static class HelseIdWebAuthBuilderExtensions
{
    public static HelseIdWebAuthBuilder? AuthBuilder { get; private set; }

    /// <summary>
    /// Use this when working with Minimal app
    /// </summary>
    public static HelseIdWebAuthBuilder AddHelseIdWebAuthentication(this WebApplicationBuilder builder)
    {
        AuthBuilder = new HelseIdWebAuthBuilder(builder.Configuration, builder.Services);
        return AuthBuilder;
    }

    /// <summary>
    /// Use this when working with non-minimal app
    /// </summary>
    public static HelseIdWebAuthBuilder AddHelseIdWebAuthentication(this IServiceCollection services, IConfiguration config)
    {
        AuthBuilder = new HelseIdWebAuthBuilder(config, services);
        return AuthBuilder;
    }

    /// <summary>
    /// Must be used after AddHelseIdWebAuthentication
    /// </summary>
    public static void UseHelseIdProtectedPaths(this IApplicationBuilder app)
    {
        if (AuthBuilder == null)
        {
            throw new NotInitializedAuthBuilderException();
        }

        AuthBuilder.UseHelseIdProtectedPaths(app);
    }

    /// <summary>
    /// Must be used after AddHelseIdWebAuthentication
    /// </summary>
    public static void UseHelseIdProtectedPaths(this IApplicationBuilder app, IReadOnlyCollection<PathString> excludeList, bool overrideDefaults = false)
    {
        if (AuthBuilder == null)
        {
            throw new NotInitializedAuthBuilderException();
        }

        AuthBuilder.UseHelseIdProtectedPaths(app, excludeList, overrideDefaults);
    }
}
