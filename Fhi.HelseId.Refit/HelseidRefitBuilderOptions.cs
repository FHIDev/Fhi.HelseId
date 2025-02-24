namespace Fhi.HelseId.Refit;

/// <summary>
/// Options for customizing the default options used when creating Refit clients
/// </summary>
public class HelseidRefitBuilderOptions
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
    /// The handler anonymizes all NIN-like requets paths and removes query strings from the log URL.
    /// </summary>
    public bool UseAnonymizationLogger { get; set; } = true;

    /// <summary>
    /// The default implementation of HttpClientFactry sets the complete URI in the logging Scope,
    /// which might contain sensitive information that we are not able to remove.
    /// We therefore remove the default logger. Set to true if you want to preserve the default logger.
    /// </summary>
    public bool PreserveDefaultLogger { get; set; } = false;
}
