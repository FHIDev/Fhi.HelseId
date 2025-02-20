using Fhi.HelseId.Common.Exceptions;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Fhi.HelseId.Blazor
{
    public static class WebApplicationExtensions
    {
        public static HelseidRefitBuilderForBlazor AddHelseIdForBlazor(this WebApplicationBuilder builder, string? configSection = null, HelseidRefitBuilderForBlazorOptions? builderOptions = null, RefitSettings? refitSettings = null)
        {
            var config = builder.Configuration
                .GetSection(configSection ?? nameof(HelseIdWebKonfigurasjon))
                .Get<HelseIdWebKonfigurasjon?>() ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon));

            return new HelseidRefitBuilderForBlazor(builder.Services, config, builderOptions, refitSettings);
        }

        public static HelseidRefitBuilderForBlazor AddHelseIdForBlazor(this IServiceCollection services, HelseIdWebKonfigurasjon config, HelseidRefitBuilderForBlazorOptions? builderOptions = null, RefitSettings? refitSettings = null)
        {
            return new HelseidRefitBuilderForBlazor(services, config, builderOptions, refitSettings);
        }

        /// <summary>
        /// Add a scoped state that will be populated using a HttpContext. This allows you to easily get context values from the anywhere.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static StateHandlerBuilder AddStateHandlers(this WebApplicationBuilder builder)
        {
            return AddStateHandlers(builder.Services);
        }

        /// <summary>
        /// Add a scoped state that will be populated using a HttpContext. This allows you to easily get values from the anywhere.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static StateHandlerBuilder AddStateHandlers(this IServiceCollection services)
        {
            var existingOptionsService = services.FirstOrDefault(x => x.ServiceType == typeof(StateHandlerOptions));
            var options = existingOptionsService?.ImplementationInstance as StateHandlerOptions;
            return new StateHandlerBuilder(services, options ?? new StateHandlerOptions());
        }

        /// <summary>
        /// Configures usage of Logout urls and Correlation Id middleware.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static WebApplication UseHelseIdForBlazor(this WebApplication app)
        {
            var options = app.Services.GetService<HelseidRefitBuilderForBlazorOptions>();
            if (options == null)
            {
                throw new Exception("You need to call builder.AddHelseIdForBlazor() before using app.UseHelseIdForBlazor()");
            }

            app.UseMiddleware<BlazortContextMiddleware>();

            if (options.UseCorrelationId)
            {
                app.UseMiddleware<CorrelationIdMiddleware>();
            }

            if (options.UseLogoutUrl)
            {
                app.Use(async (context, next) =>
                {
                    if (context.Request.Path.Equals(options.LogOutUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        await context.SignOutAsync(HelseIdContext.Scheme, new AuthenticationProperties
                        {
                            RedirectUri = options.LoggedOutRedirectUrl,
                        });

                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties
                        {
                            RedirectUri = options.LoggedOutRedirectUrl,
                        });

                        return;
                    }

                    await next();
                });
            }

            return app;
        }
    }
}
