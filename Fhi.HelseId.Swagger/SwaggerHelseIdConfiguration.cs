using System.Text.Json.Serialization;

namespace Fhi.HelseId.Swagger;

public class SwaggerHelseIdConfiguration
{
    public string TokenEndpoint { get; set; } = "";

    [JsonPropertyName("clientName")]
    public string ClientName { get; set; } = "";

    [JsonPropertyName("authority")]
    public string Authority { get; set; } = "";

    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = "";

    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; } = [];

    [JsonPropertyName("privateJwk")]
    public string PrivateJwk { get; set; } = "";
}
