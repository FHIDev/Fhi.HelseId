using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public interface IStateHandler
    {
        Task Populate(HttpContext? httpContext);
    }

    public class StateHandler : IStateHandler
    {
        private readonly StateHandlerOptions options;
        private readonly BlazorContextService contextHandler;
        private readonly IServiceProvider provider;

        public StateHandler(StateHandlerOptions options, BlazorContextService contextHandler, IServiceProvider provider)
        {
            this.options = options;
            this.provider = provider;
            this.contextHandler = contextHandler;
        }

        public async Task Populate(HttpContext? ctx)
        {
            if (ctx != null)
            {
                await PopulateWithContext(ctx);
            }
            else
            {
                await contextHandler.NewContext(async (context) =>
                {
                    await PopulateWithContext(context);
                });
            }
        }

        private async Task PopulateWithContext(HttpContext context)
        {
            foreach (var stateType in options.StateTypes)
            {
                var state = (IScopedState)provider.GetRequiredService(stateType);
                await state.Populate(context);
            }
        }
    }
}
