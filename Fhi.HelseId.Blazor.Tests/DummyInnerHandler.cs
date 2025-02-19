using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Fhi.HelseId.Blazor.Tests;

public class DummyInnerHandler : HttpClientHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get any auth header, and use its value as the content of the request
        var auth = request.Headers.FirstOrDefault(x => x.Key == "Authorization").Value?.FirstOrDefault() ?? "";

        var response = new HttpResponseMessage() { Content = new StringContent(auth) };

        // add all fhi headers in the request to the respons so we can see if they have been modified
        foreach (var h in request.Headers.Where(x => x.Key.StartsWith("fhi-")))
        {
            response.Headers.Add(h.Key, h.Value);
        }

        return Task.FromResult(response);
    }
}
