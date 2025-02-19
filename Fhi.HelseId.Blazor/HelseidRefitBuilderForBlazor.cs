using System.Text.Json;
using System.Text.Json.Serialization;
using Fhi.HelseId.Web;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Fhi.HelseId.Blazor
{
    public class HelseidRefitBuilderForBlazor
    {
        private readonly IServiceCollection services;
        private readonly HelseIdWebKonfigurasjon helseIdConfig;
        private readonly HelseidRefitBuilderForBlazorOptions builderOptions;
        private readonly List<Type> delegationHandlers = new();

        public RefitSettings RefitSettings { get; set; }

        public HelseidRefitBuilderForBlazor(
            IServiceCollection services,
            HelseIdWebKonfigurasjon config,
            HelseidRefitBuilderForBlazorOptions? builderOptions = null,
            RefitSettings? refitSettings = null)
        {
            if (OperatingSystem.IsBrowser())
            {
                throw new Exception("HelseidRefitBuilderForBlazor is only available for server side code.");
            }

            this.RefitSettings = refitSettings ?? CreateRefitSettings();
            this.builderOptions = builderOptions ?? new();
            this.services = services;
            this.helseIdConfig = config;

            services.AddStateHandlers().AddScopedState<HelseIdState>();

            services.AddScoped<BlazorContextService>();
            services.AddScoped<BlazortContextMiddleware>();
            services.AddScoped<IBlazorTokenService, BlazorTokenService>();
            services.AddSingleton(this.builderOptions);

            var factory = new ScopedHttpClientFactory(this.builderOptions.DisposeHandlers, this.builderOptions.HttpClientHandlerBuilder);
            services.AddSingleton<IScopedHttpClientFactory>(factory);

            if (this.builderOptions.UseDefaultTokenHandler)
            {
                AddHandler<BlazorTokenHandler>();
            }

            if (this.builderOptions.HtmlEncodeFhiHeaders)
            {
                AddHandler<FhiHeaderDelegationHandler>();
            }

            if (this.builderOptions.UseCorrelationId)
            {
                AddHandler<CorrelationIdHandler>();
            }

            if (this.builderOptions.UseAnonymizationLogger)
            {
                AddHandler<LoggingDelegationHandler>();
            }
        }

        /// <summary>
        /// Add delegating handlers to the Refit client. Also adds the handler to the service collection as Transient
        /// </summary>
        public HelseidRefitBuilderForBlazor AddHandler<T>()
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
        public HelseidRefitBuilderForBlazor AddRefitClient<T>(string? nameOfService = null, Func<HttpClient, HttpClient>? extra = null)
            where T : class
        {
            var name = nameOfService ?? typeof(T).Name;

            services.AddScoped((s) =>
            {
                // We are using a custom factory, since the Refit factory does not created correctly scoped TokenHandlers.
                // We need a new TokenHandler for each request to get the access token from the correct context.
                // This is BAD, as we are not using the HttpClientFactory to create the HttpClients.
                var client = s.GetRequiredService<IScopedHttpClientFactory>().CreateHttpClient(name, s, delegationHandlers);
                client.BaseAddress = helseIdConfig.UriToApiByName(name);
                extra?.Invoke(client);
                return RestService.For<T>(client, RefitSettings);
            });

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
