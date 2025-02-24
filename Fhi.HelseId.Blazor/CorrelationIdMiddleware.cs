using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Fhi.HelseId.Blazor
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        private static void AddCorrelationIdHeaderToRequest(HttpContext context, StringValues correlationId)
        {
            context.Request.Headers[CorrelationIdHandler.CorrelationIdHeaderName] = correlationId;
        }

        private static void AddCorrelationIdHeaderToResponse(HttpContext context, StringValues correlationId)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHandler.CorrelationIdHeaderName))
                {
                    context.Response.Headers.Append(CorrelationIdHandler.CorrelationIdHeaderName, correlationId);
                }

                return Task.CompletedTask;
            });
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(CorrelationIdHandler.CorrelationIdHeaderName, out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();

                AddCorrelationIdHeaderToRequest(context, correlationId);
            }

            AddCorrelationIdHeaderToResponse(context, correlationId);

            await _next(context);
        }
    }
}
