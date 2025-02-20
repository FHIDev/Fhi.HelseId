using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Common.Middleware
{
    public class UnhandledExceptionLoggerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<UnhandledExceptionLoggerMiddleware> _logger;

        public UnhandledExceptionLoggerMiddleware(RequestDelegate next, ILogger<UnhandledExceptionLoggerMiddleware> logger)
        {
            _logger = logger;
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception. Context: {RequestPath}, Method: {RequestMethod}.",
                    context.Request.Path,
                    context.Request.Method);
                throw;
            }
        }
    }

    public static class UnhandledExceptionLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseUnhandledExceptionLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UnhandledExceptionLoggerMiddleware>();
        }
    }
}
