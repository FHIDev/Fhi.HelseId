using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Fhi.HelseId.Common.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Web.Services
{
    public interface IHelseIdClientSecretHandler
    {
        public JsonWebKey GetSecurityKey();
    }

    public abstract class SecretHandlerBase : IHelseIdClientSecretHandler
    {
        public abstract JsonWebKey GetSecurityKey();

        protected SecretHandlerBase(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon)
        {
            ConfigAuth = helseIdWebKonfigurasjon;
        }

        protected IHelseIdWebKonfigurasjon ConfigAuth { get; }
    }

    /// <summary>
    /// Used when you have the Jwk in a file. The file should contain the Jwk as a string. The ClientSecret property should contain the file name
    /// </summary>
    public class HelseIdJwkFileSecretHandler : SecretHandlerBase
    {
        private JsonWebKey Secret { get; }
        public override JsonWebKey GetSecurityKey() => Secret;

        public HelseIdJwkFileSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
            var jwk = File.ReadAllText(ConfigAuth.ClientSecret);
            Secret = new JsonWebKey(jwk);
        }
    }

    /// <summary>
    /// The ClientSecret property should contain the Jwk private key as a string
    /// </summary>
    public class HelseIdJwkSecretHandler : SecretHandlerBase
    {
        private JsonWebKey Secret { get; }
        public override JsonWebKey GetSecurityKey() => Secret;

        public HelseIdJwkSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
            Secret = new JsonWebKey(ConfigAuth.ClientSecret);
        }
    }

    /// <summary>
    /// For Azure Key Vault Secret we expect ClientSecret in the format 'name of secret;uri to vault'. For example: 'MySecret;https://your-unique-key-vault-name.vault.azure.net/'
    /// </summary>
    public class HelseIdJwkAzureKeyVaultSecretHandler : SecretHandlerBase
    {
        private JsonWebKey Secret { get; }
        public override JsonWebKey GetSecurityKey() => Secret;

        public HelseIdJwkAzureKeyVaultSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
            var azureClientSettings = ConfigAuth.ClientSecret.Split(';');
            if (azureClientSettings.Length != 2)
            {
                throw new InvalidAzureKeyVaultSettingsException();
            }

            var secretClientOptions = new SecretClientOptions
            {
                Retry =
                {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };
            var secretClient = new SecretClient(new Uri(azureClientSettings[1]), new DefaultAzureCredential(), secretClientOptions);
            var secret = secretClient.GetSecret(azureClientSettings[0]);

            Secret = new JsonWebKey(secret.Value.Value);
        }
    }

    public class HelseIdNoAuthorizationSecretHandler : SecretHandlerBase
    {
        public HelseIdNoAuthorizationSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
        }

        /// <summary>
        /// Will never be called
        /// </summary>
        /// <returns></returns>
        public override JsonWebKey GetSecurityKey()
        {
            throw new NotImplementedException("This method is not used and should not be called. If this happens, there is a missing check for AuthUse");
        }
    }

    /// <summary>
    /// For selvbetjening we expect ClientSecret to be a path to a file containing the full downloaded configuration file, including the private key in JWK format
    /// </summary>
    public class HelseIdSelvbetjeningSecretHandler : SecretHandlerBase
    {
        private JsonWebKey Secret { get; }
        public override JsonWebKey GetSecurityKey() => Secret;

        public HelseIdSelvbetjeningSecretHandler(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon) : base(helseIdWebKonfigurasjon)
        {
            var selvbetjeningJson = File.ReadAllText(ConfigAuth.ClientSecret);
            var selvbetjeningConfig = JsonSerializer.Deserialize<SelvbetjeningConfig>(selvbetjeningJson);

            if (selvbetjeningConfig == null)
            {
                throw new MissingConfigurationException($"{nameof(SelvbetjeningConfig)}: Could not deserialize json or path not found.");
            }

            var jwk = HttpUtility.UrlDecode(selvbetjeningConfig.PrivateJwk);

            Secret = new JsonWebKey(jwk);
        }

        public class SelvbetjeningConfig
        {
            [JsonPropertyName("clientName")]
            public string? ClientName { get; set; }
            [JsonPropertyName("authority")]
            public string? Authority { get; set; }
            [JsonPropertyName("clientId")]
            public string? ClientId { get; set; }
            [JsonPropertyName("grantTypes")]
            public string[]? GrantTypes { get; set; }
            [JsonPropertyName("scopes")]
            public string[]? Scopes { get; set; }
            [JsonPropertyName("redirectUris")]
            public string[]? RedirectUris { get; set; }
            [JsonPropertyName("postLogoutRedirectUris")]
            public object[]? PostLogoutRedirectUris { get; set; }
            [JsonPropertyName("secretType")]
            public string? SecretType { get; set; }
            [JsonPropertyName("rsaPrivateKey")]
            public string? RsaPrivateKey { get; set; }
            [JsonPropertyName("rsaKeySizeBits")]
            public int RsaKeySizeBits { get; set; }
            [JsonPropertyName("privateJwk")]
            public string? PrivateJwk { get; set; }
            [JsonPropertyName("pkceRequired")]
            public bool PkceRequired { get; set; }
        }
    }
}
