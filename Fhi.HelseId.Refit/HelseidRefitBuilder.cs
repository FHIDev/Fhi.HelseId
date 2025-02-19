using System.Text.Json;
using System.Text.Json.Serialization;
using Fhi.HelseId.Common;
using Fhi.HelseId.Web;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Fhi.HelseId.Refit
{
    public class HelseidRefitBuilder
    {
        private readonly List<Type> delegationHandlers = new();

        private readonly IServiceCollection services;
        private readonly HelseIdWebKonfigurasjon config;
        private readonly HelseidRefitBuilderOptions builderOptions;
        private readonly RefitSettings refitSettings;

        /// <summary>
        /// Creates a new instance of HelseidRefitBuilder. This is the main entry point for setting up Refit clients
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config">Uses only the UriToApiByName method</param>
        /// <param name="builderOptions">Configure which features of the builder that will be include.</param>
        /// <param name="refitSettings">Optional. If not specified uses a default serialization setup with CamelCase, Ignorecase and enumconvertion</param>
        public HelseidRefitBuilder(
            IServiceCollection services,
            HelseIdWebKonfigurasjon config,
            HelseidRefitBuilderOptions? builderOptions = null,
            RefitSettings? refitSettings = null)
        {
            this.refitSettings = refitSettings ?? CreateRefitSettings();
            this.builderOptions = builderOptions ?? new();

            this.services = services;
            this.config = config;

            services.AddSingleton(this.builderOptions);

            if (this.builderOptions.UseDefaultTokenHandler)
            {
                AddHandler<AuthHeaderHandler>();

                if (config.UseDPoPTokens)
                {
                    services.AddTransient<IAuthorizationHeaderSetter, DPoPAuthorizationHeaderSetter>();
                }
                else
                {
                    services.AddTransient<IAuthorizationHeaderSetter, BearerAuthorizationHeaderSetter>();
                }
            }

            if (this.builderOptions.HtmlEncodeFhiHeaders)
            {
                AddHandler<FhiHeaderDelegationHandler>();
            }

            if (this.builderOptions.UseCorrelationId)
            {
                AddHandler<CorrelationIdHandler>();
                services.AddHttpContextAccessor();
            }

            if (this.builderOptions.UseAnonymizationLogger)
            {
                AddHandler<LoggingDelegationHandler>();
            }
        }

        /// <summary>
        /// Add delegating handlers to the Refit client. Also adds the handler to the service collection as Transient
        /// </summary>
        public HelseidRefitBuilder AddHandler<T>()
            where T : DelegatingHandler
        {
            var type = typeof(T);
            if (!delegationHandlers.Any(x => x == type))
            {
                delegationHandlers.Add(type);
                services.AddTransient<T>();
            }

            return this;
        }

        /// <summary>
        /// Adds a Refit client interface and which service name to bind to, and an optional extra configuration
        /// </summary>
        /// <typeparam name="T">The Refit interface definition for the Api</typeparam>
        /// <param name="nameOfService">Name of the service that will serve the Refit Api</param>
        /// <param name="extra"></param>
        /// <returns></returns>
        public HelseidRefitBuilder AddRefitClient<T>(string? nameOfService = null, Func<IHttpClientBuilder, IHttpClientBuilder>? extra = null)
            where T : class
        {
            var name = nameOfService ?? typeof(T).Name;

            var clientBuilder = services.AddRefitClient<T>(refitSettings)
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = config.UriToApiByName(name);
                });

            if (!builderOptions.PreserveDefaultLogger)
            {
                clientBuilder.RemoveAllLoggers();
            }

            foreach (var type in delegationHandlers)
            {
                clientBuilder.AddHttpMessageHandler((s) => (DelegatingHandler)s.GetRequiredService(type));
            }

            extra?.Invoke(clientBuilder);

            return this;
        }

        private static RefitSettings CreateRefitSettings()
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            var refitSettings = new RefitSettings()
            {
                ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions),
            };

            return refitSettings;
        }
    }
}
