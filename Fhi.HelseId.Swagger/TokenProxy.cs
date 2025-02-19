using System.Web;
using Fhi.HelseId.Common.Constants;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Swagger;

public interface ITokenProxy
{
    Task<string?> RequestToken(Dictionary<string, string?> requestParameters);
}

public class TokenProxy : ITokenProxy
{
    private readonly SwaggerHelseIdConfiguration _swaggerHelseIdConfiguration;
    private readonly HttpClient _tokenClient;

    public TokenProxy(SwaggerHelseIdConfiguration swaggerHelseIdConfiguration)
    {
        _swaggerHelseIdConfiguration = swaggerHelseIdConfiguration;
        _tokenClient = new HttpClient();
    }

    public async Task<string?> RequestToken(Dictionary<string, string?> requestParameters)
    {
        var config = new HelseIdWebKonfigurasjon
        {
            ClientId = _swaggerHelseIdConfiguration.ClientId,
            Authority = _swaggerHelseIdConfiguration.Authority,
        };

        var jwk = HttpUtility.UrlDecode(_swaggerHelseIdConfiguration.PrivateJwk);
        var jwkSecurityKey = new JsonWebKey(jwk);

        var clientAssertion = ClientAssertion.Generate(config.ClientId, config.Authority, jwkSecurityKey);

        requestParameters[OpenIdConnectParameterNames.ClientId] = config.ClientId;
        requestParameters[OpenIdConnectParameterNames.ClientAssertionType] = OAuthConstants.JwtBearerClientAssertionType;
        requestParameters[OpenIdConnectParameterNames.ClientAssertion] = clientAssertion;

        var tokenUrl = $"{_swaggerHelseIdConfiguration.Authority}/connect/token";
        var result = await _tokenClient.PostAsync(tokenUrl, new FormUrlEncodedContent(requestParameters));

        var jsonResult = await result.Content.ReadAsStringAsync();

        return jsonResult;
    }
}
