﻿using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Builder;

namespace Fhi.HelseId.Web.ExtensionMethods;

public static class HelseIdWebAuthBuilderExtensions
{
    public static HelseIdWebAuthBuilder AddHelseIdWebAuthentication(this WebApplicationBuilder builder)
    {
        var authBuilder = new HelseIdWebAuthBuilder(builder.Configuration, builder.Services);
        return authBuilder;
    }

    /// <summary>
    /// The ClientSecret property should contain the Jwk private key as a string
    /// </summary>
    public static HelseIdWebAuthBuilder UseJwkKeySecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = new HelseIdJwkSecretHandler();
        return authBuilder;
    }

    /// <summary>
    /// Used when you have the Jwk in a file. The file should contain the Jwk as a string. The ClientSecret property should contain the file name
    /// </summary>
    public static HelseIdWebAuthBuilder UseJwkKeyFileSecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = new HelseIdJwkFileSecretHandler();
        return authBuilder;
    }
    /// <summary>
    /// For selvbetjening we expect ClientSecret to be a path to a file containing the full downloaded configuration file, including the private key in JWK format
    /// </summary>
    public static HelseIdWebAuthBuilder UseSelvbetjeningFileSecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = new HelseIdSelvbetjeningSecretHandler();
        return authBuilder;
    }
    /// <summary>
    /// For Azure Key Vault Secret we expect ClientSecret in the format 'name of secret;uri to vault'. For example: 'MySecret;https://your-unique-key-vault-name.vault.azure.net/'
    /// </summary>
    public static HelseIdWebAuthBuilder UseAzureKeyVaultSecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = new HelseIdJwkAzureKeyVaultSecretHandler();
        return authBuilder;
    }

    /// <summary>
    /// Use when a shared secret key is in the CLientSecret property
    /// </summary>
    public static HelseIdWebAuthBuilder UseSharedSecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = new HelseIdSharedSecretHandler();
        return authBuilder;
    }

    /// <summary>
    /// End a fluent series with this to create the authentication handlers. It returns the builder which can be further used later if needed, otherwise ignore the return.
    /// This sets up authentication and authorization services, and adds the controllers. You still need to call app.UseAuthentication() and app.UseAuthorization() to enable the middleware.
    /// </summary>
    public static HelseIdWebAuthBuilder Build(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.AddHelseIdWebAuthentication();
        return authBuilder;
    }


}