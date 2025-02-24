namespace Fhi.HelseId.Blazor
{
    /// <summary>
    /// Options for customizing the default options used when creating Refit clients
    /// </summary>
    public class HelseidRefitBuilderForBlazorOptions
    {
        /// <summary>
        /// Adds the default token handler from Fhi.HelseId, HttpAuthHandler.
        /// Set to false if you wish to use a custom token handler.
        /// </summary>
        public bool UseDefaultTokenHandler { get; set; } = true;

        /// <summary>
        /// Adds propagation and handling of correlation ids to all Refit-Requests.
        /// Remember to add "app.UseCorrelationId()" in your startup code.
        /// </summary>
        public bool UseCorrelationId { get; set; } = true;

        /// <summary>
        /// Html-encodes all headers starting with the prefix "fhi-".
        /// This is useful when using headers like "fhi-organization-name", which might contain
        /// illegal HTTP header characters.
        /// </summary>
        public bool HtmlEncodeFhiHeaders { get; set; } = true;

        /// <summary>
        /// Adds logging to all requests using LoggingDelegationHandler.
        /// The handler anonymizes all NIN-like request paths and removes query strings from the log URL.
        /// </summary>
        public bool UseAnonymizationLogger { get; set; } = true;

        /// <summary>
        /// Function for retrieving a correlation id
        /// </summary>
        public Func<IServiceProvider, string>? CustomCorrelationIdFunc { get; set; }

        /// <summary>
        /// To be able to access the http context to log out of a blazor app
        /// we need to do this from middleware where the HttpContext is available.
        /// To trigger it, be sure to force a reload when navigating to the logout url
        /// f.ex:  NavManager.NavigateTo($"/logout", forceLoad: true);
        /// </summary>
        public bool UseLogoutUrl { get; set; } = true;

        /// <summary>
        /// The url which will trigger a logout
        /// </summary>
        public string LogOutUrl { get; set; } = "/logout";

        /// <summary>
        /// The url the user will be redirected to after logging out
        /// </summary>
        public string LoggedOutRedirectUrl { get; set; } = "/loggedout";

        /// <summary>
        /// Set a http client handler builder to be used for creating transient clients
        /// </summary>
        public Func<string, HttpClientHandler> HttpClientHandlerBuilder { get; set; } = (name) => new HttpClientHandler();

        /// <summary>
        /// Set to true if the inner handler of the default HttpClientHandlerBuilder should be disposed of by HttpClient.Dispose; if you intend to reuse the inner handler.
        /// </summary>
        public bool DisposeHandlers { get; set; } = true;
    }
}
