using System;
using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Exceptions;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.DPoP;
using Fhi.HelseId.Web.Handlers;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
using Fhi.HelseId.Web.Middleware;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Fhi.HelseId.Web.ExtensionMethods;

public class HelseIdWebAuthBuilder
{
    private readonly IServiceCollection _services;
    private IConfiguration _configuration { get; }
    private readonly IConfigurationSection _helseIdWebKonfigurasjonSection;
    public IHelseIdWebKonfigurasjon HelseIdWebKonfigurasjon { get; }
    public RedirectPagesKonfigurasjon RedirectPagesKonfigurasjon { get; }
    public IHelseIdSecretHandler SecretHandler { get; set; }

    /// <summary>
    /// Checks the HelseIdWebKonfigurasjon.AuthUse property
    /// Returns false if it doesn't exist.
    /// </summary>
    public bool UseAuthentication => HelseIdWebKonfigurasjon?.AuthUse ?? false;

    public HelseIdWebAuthBuilder(IConfiguration configuration, IServiceCollection services)
    {
        _services = services;
        _configuration = configuration;
        _helseIdWebKonfigurasjonSection = _configuration.GetSection(nameof(HelseIdWebKonfigurasjon));
        if (_helseIdWebKonfigurasjonSection == null)
            throw new MissingConfigurationException($"Missing required configuration section {nameof(HelseIdWebKonfigurasjon)}");
        var helseIdWebKonfigurasjon = _helseIdWebKonfigurasjonSection.Get<HelseIdWebKonfigurasjon>();
        if (helseIdWebKonfigurasjon == null)
            throw new MissingConfigurationException($"Missing required configuration {nameof(HelseIdWebKonfigurasjon)}");
        HelseIdWebKonfigurasjon = helseIdWebKonfigurasjon;
        RedirectPagesKonfigurasjon = HelseIdWebKonfigurasjon.RedirectPagesKonfigurasjon;
        SecretHandler = new HelseIdNoAuthorizationSecretHandler(helseIdWebKonfigurasjon); // Default
    }

    /// <summary>
    /// Add this to the services section
    /// </summary>
    /// <param name="configureMvc"></param>
    /// <param name="configureAuthentication"></param>
    /// <returns></returns>
    public IMvcBuilder? AddHelseIdWebAuthentication(
        Action<MvcOptions>? configureMvc = null,
        ConfigureAuthentication? configureAuthentication = null)
    {
        if (HelseIdWebKonfigurasjon.AuthUse)
        {
            _services.AddSingleton<IGodkjenteHprKategoriListe, NoHprApprovals>();
            if (HelseIdWebKonfigurasjon.RequireValidHprAuthorization)
            {
                _services.AddScoped<IAuthorizationHandler, HprGodkjenningAuthorizationHandler>();
                _services.AddScoped<IAuthorizationHandler, HprAuthorizationHandler>();
            }
            else if (HelseIdWebKonfigurasjon.RequireHprNumber)
                _services.AddScoped<IAuthorizationHandler, HprAuthorizationHandler>();

            _services.AddSingleton<IWhitelist>(HelseIdWebKonfigurasjon.Whitelist);
            _services.AddSingleton<IAutentiseringkonfigurasjon>(HelseIdWebKonfigurasjon);
            _services.AddMemoryCache();
            _services.AddScoped<IHprService, HprService>();
            _services.AddSingleton<IAuthorizationHandler, SecurityLevelClaimHandler>();
            JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

            if (HelseIdWebKonfigurasjon.UseDPoPTokens)
            {
                _services.AddDistributedMemoryCache();
                _services.AddTransient<IDPoPTokenCreator, DPoPTokenCreator>();
                _services.AddTransient<INonceStore, NonceStore>();
                _services.AddSingleton<IProofRedirector, JwtThumbprintAttacher>();
                _services.AddTransient<BackchannelHandler>();

                _services.ConfigureOptions<BackchannelConfiguration>();
                _services.AddTransient<RefreshTokenBackchannelHandler>();
                _services.AddHttpClient<TokenEndpointService>()
                    .AddHttpMessageHandler<RefreshTokenBackchannelHandler>();
                _services.AddTransient<IAuthorizationHeaderSetter, DPoPAuthorizationHeaderSetter>();
            }
            else
            {
                _services.AddHostedService<DPoPComplianceWarning>();
                _services.AddHttpClient<TokenEndpointService>();
                _services.AddTransient<IAuthorizationHeaderSetter, BearerAuthorizationHeaderSetter>();
            }
        }
        else
        {
            _services.AddSingleton<IAuthorizationHandler, NoAuthorizationHandler>();
            _services.AddAuthentication("NoAuthentication")
                .AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>("NoAuthentication", null);
        }
        _services.AddScoped<ICurrentUser, CurrentHttpUser>();
        _services.AddSingleton(HelseIdWebKonfigurasjon);
        _services.Configure<HelseIdWebKonfigurasjon>(_helseIdWebKonfigurasjonSection);
        _services.AddHttpContextAccessor();
        _services.AddSingleton(SecretHandler);

        (var authorizeFilter, string policyName) = AddAuthentication(configureAuthentication);

        AddControllers(configureMvc, authorizeFilter);

        return MvcBuilder;
    }

    public HelseIdWebAuthBuilder AddControllers(Action<MvcOptions>? configureMvc, AuthorizeFilter? authorizeFilter)
    {
        MvcBuilder = _services.AddControllers(config =>
        {
            //Unsure about this
            if (HelseIdWebKonfigurasjon.AuthUse && authorizeFilter is not null)
            {
                config.Filters.Add(authorizeFilter!);
            }

            configureMvc?.Invoke(config);
        });

        return this;
    }

    /// <summary>
    /// The ClientSecret property should contain the Jwk private key as a string
    /// </summary>
    public HelseIdWebAuthBuilder UseJwkKeySecretHandler()
    {
        SecretHandler = HelseIdWebKonfigurasjon.AuthUse ? new HelseIdJwkSecretHandler(HelseIdWebKonfigurasjon) : new HelseIdNoAuthorizationSecretHandler(HelseIdWebKonfigurasjon);
        return this;
    }

    /// <summary>
    /// Used when you have the Jwk in a file. The file should contain the Jwk as a string. The ClientSecret property should contain the file name
    /// </summary>
    public HelseIdWebAuthBuilder UseJwkKeyFileSecretHandler()
    {
        SecretHandler = HelseIdWebKonfigurasjon.AuthUse ? new HelseIdJwkFileSecretHandler(HelseIdWebKonfigurasjon) : new HelseIdNoAuthorizationSecretHandler(HelseIdWebKonfigurasjon);
        return this;
    }

    /// <summary>
    /// For selvbetjening we expect ClientSecret to be a path to a file containing the full downloaded configuration file, including the private key in JWK format
    /// </summary>
    public HelseIdWebAuthBuilder UseSelvbetjeningFileSecretHandler()
    {
        SecretHandler = HelseIdWebKonfigurasjon.AuthUse ? new HelseIdSelvbetjeningSecretHandler(HelseIdWebKonfigurasjon) : new HelseIdNoAuthorizationSecretHandler(HelseIdWebKonfigurasjon);
        return this;
    }

    /// <summary>
    /// For Azure Key Vault Secret we expect ClientSecret in the format 'name of secret;uri to vault'. For example: 'MySecret;https://your-unique-key-vault-name.vault.azure.net/'
    /// </summary>
    public HelseIdWebAuthBuilder UseAzureKeyVaultSecretHandler()
    {
        SecretHandler = HelseIdWebKonfigurasjon.AuthUse ? new HelseIdJwkAzureKeyVaultSecretHandler(HelseIdWebKonfigurasjon) : new HelseIdNoAuthorizationSecretHandler(HelseIdWebKonfigurasjon);
        return this;
    }

    /// <summary>
    /// This property is set by the AddControllers method
    /// </summary>
    public IMvcBuilder? MvcBuilder { get; private set; }

    /// <summary>
    /// End a fluent series with this to create the authentication handlers. It returns the builder which can be further used later if needed, otherwise ignore the return.
    /// This sets up authentication and authorization services, and adds the controllers. You still need to call app.UseAuthentication() and app.UseAuthorization() to enable the middleware.
    /// </summary>
    public HelseIdWebAuthBuilder Build(Action<MvcOptions>? configureMvc = null,
        ConfigureAuthentication? configureAuthentication = null)
    {
        AddHelseIdWebAuthentication(configureMvc, configureAuthentication);
        return this;
    }

    /// <summary>
    /// Add Authentication to the services section
    /// </summary>
    private AuthenticationBuilder AddHelseIdAuthentication()
    {
        var builder = _services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = HelseIdContext.Scheme;
        });

        return builder;
    }

    protected virtual (AuthorizeFilter AuthorizeFilter, string PolicyName) AddAuthentication(ConfigureAuthentication? configureAuthentication = null)
    {
        const double tokenRefreshBeforeExpirationTime = 2;
        if (HelseIdWebKonfigurasjon.AuthUse)
        {
            AddHelseIdAuthentication()
                .AddCookie(options =>
                {
                    options.DefaultHelseIdOptions(HelseIdWebKonfigurasjon, RedirectPagesKonfigurasjon);

                    configureAuthentication?.ConfigureCookieAuthentication?.Invoke(options);
                })
                .AddOpenIdConnect(HelseIdContext.Scheme, options =>
                {
                    options.DefaultHelseIdOptions(HelseIdWebKonfigurasjon, RedirectPagesKonfigurasjon, SecretHandler);

                    configureAuthentication?.ConfigureOpenIdConnect?.Invoke(options);
                })
                .AddAutomaticTokenManagement(options => options.DefaultHelseIdOptions(tokenRefreshBeforeExpirationTime));   // For å kunne ha en lengre sesjon,  håndterer refresh token
        }

        (var authPolicy, string policyName) = AddHelseIdAuthorizationPolicy();

        return (new AuthorizeFilter(authPolicy), policyName);
    }

    /// <summary>
    /// Add this to the app section, used to trigger the authentication login process for files and endpoints that are otherwise not protected. Enable this by setting UseProtectedPaths. 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="excludeList"></param>
    /// <param name="overrideDefaults"></param>
    public void UseHelseIdProtectedPaths(IApplicationBuilder app,
        IReadOnlyCollection<PathString> excludeList, bool overrideDefaults = false)
    {
        if (!HelseIdWebKonfigurasjon.AuthUse || !HelseIdWebKonfigurasjon.UseProtectedPaths) return;
        var excluded = overrideDefaults
            ? new List<PathString>()
            : new()
            {
                "/favicon.ico",
                RedirectPagesKonfigurasjon.Error,
                RedirectPagesKonfigurasjon.Forbidden,
                RedirectPagesKonfigurasjon.LoggedOut,
                RedirectPagesKonfigurasjon.Statuscode,
                "/Account/Login",
                "/Account/Logout"
            };
        if (excludeList.Any())
            excluded.AddRange(excludeList);

        app.UseProtectPaths(
            new ProtectPathsOptions(DeterminePrecedingPolicy(), RedirectPagesKonfigurasjon.Forbidden)
            {
                Exclusions = excluded
            });
    }

    public void UseHelseIdProtectedPaths(IApplicationBuilder app) => UseHelseIdProtectedPaths(app, new List<PathString>());

    /// <summary>
    /// Adds Authorization, replaces services.AddAuthorization()
    /// </summary>
    /// <returns></returns>
    public (AuthorizationPolicy AuthPolicy, string PolicyName) AddHelseIdAuthorizationPolicy()
    {
        if (!HelseIdWebKonfigurasjon.AuthUse)
        {
            var noAuthorizationPolicy = new AuthorizationPolicyBuilder().AddRequirements(new NoAuthorizationRequirement()).Build();
            return (noAuthorizationPolicy, Policies.NoAuthorization);
        }

        var authenticatedPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        var hidOrApiPolicy = new AuthorizationPolicyBuilder()
            .Combine(authenticatedPolicy)
            .AddRequirements(new SecurityLevelOrApiRequirement())
            .Build();

        if (HelseIdWebKonfigurasjon.RequireHprNumber)
        {
            var hprNumberPolicyBuilder = new AuthorizationPolicyBuilder()
                .Combine(hidOrApiPolicy);
            hprNumberPolicyBuilder.Requirements.Add(new HprAuthorizationRequirement());
            var hprNumberPolicy = hprNumberPolicyBuilder.Build();

            if (HelseIdWebKonfigurasjon.RequireValidHprAuthorization)
            {
                var policy = new AuthorizationPolicyBuilder()
                    .Combine(hprNumberPolicy);
                policy.Requirements.Add(new HprGodkjenningAuthorizationRequirement());
                var hprGodkjenningPolicy = policy.Build();
                _services.AddAuthorization(config =>
                {
                    config.AddPolicy(Policies.HidOrApi, hidOrApiPolicy);
                    config.AddPolicy(Policies.HprNummer, hprNumberPolicy);
                    config.AddPolicy(Policies.GodkjentHprKategoriPolicy, hprGodkjenningPolicy);
                    config.DefaultPolicy = hprGodkjenningPolicy;
                });

                return (hprGodkjenningPolicy, Policies.GodkjentHprKategoriPolicy);
            }

            _services.AddAuthorization(config =>
            {
                config.AddPolicy(Policies.HidOrApi, hidOrApiPolicy);
                config.AddPolicy(Policies.HprNummer, hprNumberPolicy);
                config.DefaultPolicy = hprNumberPolicy;
            });

            return (hprNumberPolicy, Policies.HprNummer);
        }

        _services.AddAuthorization(config =>
        {
            config.AddPolicy(Policies.HidOrApi, hidOrApiPolicy);
            config.DefaultPolicy = hidOrApiPolicy;
        });

        return (hidOrApiPolicy, Policies.HidOrApi);
    }

    /// <summary>
    /// Determine the preceding policy from configuration.
    /// Will return the most "restrictive" policy configured, or 
    /// Policies.HidOrApi if no policies are configured.
    /// </summary>
    /// <returns></returns>
    private string DeterminePrecedingPolicy()
        => new[]
            {
                new { PolicyActive = HelseIdWebKonfigurasjon.RequireHprNumber && HelseIdWebKonfigurasjon.RequireValidHprAuthorization, Policy = Policies.GodkjentHprKategoriPolicy},
                new { PolicyActive = HelseIdWebKonfigurasjon.RequireHprNumber, Policy = Policies.HprNummer },
                new { PolicyActive = true, Policy = Policies.HidOrApi },
                new { PolicyActive = true, Policy = Policies./*Hid*/Authenticated }
            }
            .ToList()
            .First(p => p.PolicyActive).Policy;

    /// <summary>
    /// Use this to add the HelseId Api access token handling to the app.
    /// </summary>
    public HelseIdWebAuthBuilder AddOutgoingApis()
    {
        _services.AddTransient<AuthHeaderHandler>();

        return this;
    }

    public HelseIdWebAuthBuilder WithHttpClients()
    {
        foreach (var api in HelseIdWebKonfigurasjon.Apis)
        {
            if (HelseIdWebKonfigurasjon is { AuthUse: true, UseApis: true })
                AddHelseIdApiServices(api);
            else
                AddHelseIdApiServicesNoAuth(api);
        }

        return this;
    }

    private IHttpClientBuilder AddHelseIdApiServices(IApiOutgoingKonfigurasjon api)
        => _services.AddHttpClient(api.Name, configureClient: client =>
        {
            client.BaseAddress = api.Uri;
            client.Timeout = TimeSpan.FromMinutes(10);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();

    private IHttpClientBuilder AddHelseIdApiServicesNoAuth(IApiOutgoingKonfigurasjon api)
        => _services.AddHttpClient(api.Name, client =>
        {
            client.BaseAddress = api.Uri;
            client.Timeout = TimeSpan.FromMinutes(10);
        });
}

public class NoHprApprovals : GodkjenteHprKategoriListe
{
    public NoHprApprovals()
    {
    }
}