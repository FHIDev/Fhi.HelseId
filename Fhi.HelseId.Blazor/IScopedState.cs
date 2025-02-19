using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Blazor
{
    public interface IScopedState
    {
        Task Populate(HttpContext context);
    }
}
