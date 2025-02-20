using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public class StateHandlerBuilder
    {
        public StateHandlerOptions Options;

        private IServiceCollection _services;

        public StateHandlerBuilder(IServiceCollection services, StateHandlerOptions options)
        {
            Options = options;
            _services = services;

            services.AddSingleton(Options);
            services.AddScoped<IStateHandler, StateHandler>();
        }

        public StateHandlerBuilder AddScopedState<T>()
            where T : class, IScopedState
        {
            Options.StateTypes.Add(typeof(T));
            _services.AddScoped<T>();
            return this;
        }
    }

    public class StateHandlerOptions
    {
        public List<Type> StateTypes = new();
    }
}
