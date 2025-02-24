using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fhi.HelseId.Common.DPoP;

namespace Fhi.HelseId.Web.DPoP;

public class RefreshTokenBackchannelHandler(
    IDPoPTokenCreator tokenHelper)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SetDPoPHeader(request);

        var response = await base.SendAsync(request, cancellationToken);
        var nonce = GetNonce(response.Headers);

        // If the STS returned a 400 bad request and provided a nonce, we can resend the request
        // with a DPoP header using the provided nonce from the STS.
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && !string.IsNullOrEmpty(nonce))
        {
            response.Dispose();
            SetDPoPHeader(request, nonce);

            return await base.SendAsync(request, cancellationToken);
        }

        return response;
    }

    private async void SetDPoPHeader(HttpRequestMessage request, string? nonceValue = null)
    {
        var proof = await tokenHelper.CreateSignedToken(
            method: request.Method,
            url: request.RequestUri!.Scheme + "://" + request.RequestUri!.Authority + request.RequestUri!.LocalPath,
            nonce: nonceValue);

        request.Headers.Remove(DPoPHttpHeaders.ProofHeaderName);
        request.Headers.Add(DPoPHttpHeaders.ProofHeaderName, proof);
    }

    private static string? GetNonce(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        return headers
            .FirstOrDefault(x => x.Key == DPoPHttpHeaders.NonceHeaderName)
            .Value?.FirstOrDefault();
    }
}
