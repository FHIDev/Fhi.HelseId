using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Fhi.HelseId.Api.Authorization;
using Fhi.HelseId.Api.DPoP;
using Fhi.HelseId.Api.Handlers;
using Fhi.HelseId.Api.Services;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Fhi.HelseId.Api.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Use this for setting up ingoing api authentication (with an access token)
    /// This extension enables multiple scopes defined in the configuration by one handler, and single scope by another
    /// </summary>
    public static void AddHelseIdApiAuthentication(this IServiceCollection services,
        IHelseIdApiKonfigurasjon config)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton(config);

        if (config.AuthUse)
        {
            services.AddScoped<ICurrentUser, CurrentHttpUser>();
            if (config.ApiScope.Contains(',')) // We know there are multiple scopes if a komma is present
                services.AddSingleton<IAuthorizationHandler, ApiMultiScopeHandler>();
            else
                services.AddSingleton<IAuthorizationHandler, ApiSingleScopeHandler>();
            services.AddScoped<IAccessTokenProvider, HttpContextAccessTokenProvider>();

            if (config.AllowDPoPTokens || config.RequireDPoPTokens)
            {
                services.AddDistributedMemoryCache();
                services.AddSingleton<IReplayCache, InMemoryReplayCache>();
                services.AddTransient<IDPoPProofValidator, DPoPProofValidator>();
                services.AddTransient<IJwtBearerDPoPTokenHandler, JwtBearerDPoPTokenHandler>();
            }

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddHelseIdJwtBearer(config);

            if (config.RequireContextIdentity)
                services.AddHelseIdAuthorization(config);
            else
                services.AddHelseIdApiAccessOnlyAuthorization(config);
        }
    }

    /// <summary>
    /// Use this for either User or Client credentials
    /// </summary>
    public static bool AddHelseIdAuthorizationControllers(this IServiceCollection services,
        IAutentiseringkonfigurasjon config)
    {
        if (!config.AuthUse)
        {
            return false;
        }

        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
        services.AddControllers(cfg => { cfg.Filters.Add(new AuthorizeFilter(Policies.HidOrApi)); })
            .AddJsonOptions(
                options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
        return true;
    }
}
