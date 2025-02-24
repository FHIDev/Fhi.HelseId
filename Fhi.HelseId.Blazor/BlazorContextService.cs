using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Fhi.HelseId.Blazor;

/*
 * This code aims to solve some Blazor<>HelseId problems
 * - The lifetime of the helse-id access token is max 600s, so it can time out before the user makes new Http-requests
 * - The code that should try to refresh the access token relies on IHttpContextAccessor, which is not available in Blazor
 * = We only have a valid Access&Refresh token for 600s after the first HelseId login, even though the cookie is valid for a lot longer.
 *
 * As Blazor uses SignalR, we will not normally have access to the HttpContext during the SPA's lifetime.
 * If we try to use the refresh_token to get a new access_token that will succeed and we can store it in the UserState,
 * but we are then not able to update  the refresh_token stored in the users cookie since we do not have a valid HttpContext to access the cookie through.
 * If they refresh the page they will then have an invalid refresh_token (and access_token) from the old cookie and all requests will fail.
 *
 * Read more about stale refresh tokens in blazor:
 * https://stackoverflow.com/questions/72868249/how-to-handle-user-oidc-tokens-in-blazor-server-when-the-browser-is-refreshed-an
 *
 * Instead of the above proposed solution, the code beneth aims to solve the HttpContext-problem once and for all, so we will not have further problems with it in the future.
 * We do this by creating a service which we can use to create a NEW HttpContext where we have access to reading/setting cookies.
*/

public class BlazorContextService
{
    public const string NEW_CONTEXT_URL = "/_blazor/httpcontext";

    private static ConcurrentDictionary<string, Func<HttpContext, Task>> _actionMap = new();
    private readonly IJSRuntime jsRuntime;

    public BlazorContextService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public async Task ConnectContext(HttpContext context, string id)
    {
        var action = _actionMap[id];
        await action(context);
        _actionMap.TryRemove(id, out _);
    }

    public async Task NewContext(Func<HttpContext, Task> action)
    {
        var id = Guid.NewGuid().ToString();
        _actionMap[id] = action;
        await jsRuntime.InvokeVoidAsync("fetch", $"{NEW_CONTEXT_URL}/{id}");
    }
}

public class BlazortContextMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.Value?.StartsWith(BlazorContextService.NEW_CONTEXT_URL) == true)
        {
            var id = context.Request.Path.Value.Split('/').Last();
            var service = context.RequestServices.GetRequiredService<BlazorContextService>();
            await service.ConnectContext(context, id);
            return;
        }

        await next(context);
    }
}
