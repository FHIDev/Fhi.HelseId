using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Refit;

public class CorrelationIdHandler : DelegatingHandler
{
    public const string CorrelationIdHeaderName = "X-Correlation-ID";

    private readonly IHttpContextAccessor httpContextAccessor;

    public CorrelationIdHandler(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();

        var context = httpContextAccessor.HttpContext;
        if (context != null)
        {
            var corrFromContext = context.Request.Headers.FirstOrDefault(x => x.Key == CorrelationIdHeaderName).Value.FirstOrDefault();
            if (corrFromContext != null)
            {
                correlationId = corrFromContext;
            }
        }

        if (request.Headers.TryGetValues(CorrelationIdHeaderName, out var values))
        {
            correlationId = values!.First();
        }
        else
        {
            request.Headers.Add(CorrelationIdHeaderName, correlationId);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (!response.Headers.TryGetValues(CorrelationIdHeaderName, out _))
        {
            response.Headers.Add(CorrelationIdHeaderName, correlationId);
        }

        return response;
    }
}
