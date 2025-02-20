using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public interface IScopedHttpClientFactory
    {
        /// <summary>
        /// Creates a HttpClient, with a call chain of handlers specified by DelegationHandlers.
        /// The innermost handler is created using the spcified HttpClientHandlerBuilder.
        /// If no HttpClientHandlerBuilder is specified, "new HttpClientHandler()" will be used.
        /// </summary>
        /// <param name="name">Client name. Not needed for default HttpClientHandler, but might be used in a custom httpClientHandlerBuilder.</param>
        /// <param name="provider">The scoped service provider to create handlers from.</param>
        /// <param name="delegationHandlers">A list of handlers to use for the client.</param>
        /// <returns></returns>
        HttpClient CreateHttpClient(string name, IServiceProvider provider, List<Type> delegationHandlers);
    }

    public class ScopedHttpClientFactory : IScopedHttpClientFactory
    {
        private readonly Func<string, HttpClientHandler> httpClientHandlerBuilder;
        private readonly bool disposeHandlers;

        /// <summary>
        /// HttpClientFactory for creating scoped HttpClients with a list of delegation handlers.
        /// </summary>
        /// <param name="disposeHandlers">Set a http client handler builder to be used for creating transient clients</param>
        /// <param name="httpClientHandlerBuilder">true if the inner handler should be disposed of by HttpClient.Dispose; if you intend to reuse the inner handler.</param>
        public ScopedHttpClientFactory(bool disposeHandlers = true, Func<string, HttpClientHandler>? httpClientHandlerBuilder = null)
        {
            this.httpClientHandlerBuilder = httpClientHandlerBuilder ?? ((name) => new HttpClientHandler());
            this.disposeHandlers = disposeHandlers;
        }

        public HttpClient CreateHttpClient(string name, IServiceProvider provider, List<Type> delegationHandlers)
        {
            if (delegationHandlers.Count == 0)
            {
                return new HttpClient();
            }

            var mainHandler = (DelegatingHandler)provider.GetRequiredService(delegationHandlers.First());
            var outer = mainHandler;
            foreach (var handlerType in delegationHandlers.Skip(1))
            {
                if (outer.InnerHandler != null)
                {
                    throw new Exception($"{outer.GetType()} should not have a inner handler when.");
                }

                outer.InnerHandler = (DelegatingHandler)provider.GetRequiredService(handlerType);
                outer = (DelegatingHandler)outer.InnerHandler;
            }

            outer.InnerHandler = httpClientHandlerBuilder.Invoke(name);

            // Attempt to clean up handlers. They may still linger awaiting for closing of sockets
            return new HttpClient(mainHandler, disposeHandlers);
        }
    }
}
