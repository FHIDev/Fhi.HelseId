using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Common.Middleware
{
    public class RedirectOnExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _redirectPage;
        private readonly ILogger<RedirectOnExceptionMiddleware> _logger;

        public RedirectOnExceptionMiddleware(RequestDelegate next, string redirectPage, ILogger<RedirectOnExceptionMiddleware> logger)
        {
            _logger = logger;
            _next = next;
            _redirectPage = redirectPage;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handling exception by redirecting to {page}.", _redirectPage);
                context.Response.Redirect(_redirectPage);
            }
        }
    }

    public static class RedirectOnExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseRedirectOnException(
            this IApplicationBuilder builder,
            string redirectPage)
        {
            return builder.UseMiddleware<RedirectOnExceptionMiddleware>(redirectPage);
        }
    }
}