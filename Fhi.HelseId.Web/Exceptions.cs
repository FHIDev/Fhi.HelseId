using System;

namespace Fhi.HelseId.Web;

public class ConfigurationException(string message) : Exception(message);

public class InvalidAzureKeyVaultSettingsException() : Exception(StandardMessage)
{
    private const string StandardMessage = "For Azure Key Vault Secret we expect ClientSecret in the format <name of secret>;<uri to vault>. For example: 'MySecret;https://<your-unique-key-vault-name>.vault.azure.net/'";
}

public class InvalidApiNameException : Exception
{
    public InvalidApiNameException() { }
    public InvalidApiNameException(string message) : base(message) { }
    public InvalidApiNameException(string message, Exception inner) : base(message, inner) { }
}
