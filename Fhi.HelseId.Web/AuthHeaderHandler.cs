using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Exceptions;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fhi.HelseId.Common;

/// <summary>
/// This is to be used with either User or Client credentials
/// See https://github.com/reactiveui/refit#bearer-authentication
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class AuthHeaderHandler : DelegatingHandler
{
    public const string AnonymousOptionKey = "Anonymous";

    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<AuthHeaderHandler> _logger;
    private readonly ICurrentUser _user;
    private readonly HelseIdWebKonfigurasjon config;
    private readonly IAuthorizationHeaderSetter _authorizationHeaderSetter;

    public AuthHeaderHandler(IHttpContextAccessor contextAccessor,
        ILogger<AuthHeaderHandler> logger,
        ICurrentUser user,
        IOptions<HelseIdWebKonfigurasjon> options,
        IAuthorizationHeaderSetter authorizationHeaderSetter)
    {
        config = options.Value;
        _contextAccessor = contextAccessor;
        _logger = logger;
        _user = user;
        _authorizationHeaderSetter = authorizationHeaderSetter;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Options.Any(x => x.Key == AnonymousOptionKey))
        {
            _logger.LogTrace("Skipping Access token because of anonymous HttpRequestMessage options.");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var ctx = _contextAccessor.HttpContext ?? throw new NoContextException();
        _logger.LogTrace("Starting.");
        var token = await ctx.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

        if (token == null)
        {
            _logger.LogError("No access token found in context. Make sure you have added the" +
                " AddTokenManagement() to your Startup.cs.");
        }
        else
        {
            _logger.LogTrace("Found access token in context (hash: {hash}).", token.GetHashCode());
        }

        if (!string.IsNullOrEmpty(token))
        {
            await _authorizationHeaderSetter.SetAuthorizationHeader(request, token);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Request to {url} failed with status code {statusCode}.", request.RequestUri, response.StatusCode);
        }

        return response;
    }

    private string GetDebuggerDisplay() => ToString() ?? "";
}