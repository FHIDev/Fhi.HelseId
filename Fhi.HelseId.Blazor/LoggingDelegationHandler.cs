using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Blazor;

public class LoggingDelegationHandler : DelegatingHandler
{
    private const string LogMessage = "Requested HTTP {RequestMethod} {Uri} in {Elapsed}ms with response {StatusCode} {Reason} with CorrelationId {CorrelationId}";
    private const string LogMessageError = "Requested HTTP {RequestMethod} {Uri} in {Elapsed}ms with exception {Exception} with CorrelationId {CorrelationId}";

    private readonly ILogger<LoggingDelegationHandler> logger;

    public LoggingDelegationHandler(ILogger<LoggingDelegationHandler> logger)
    {
        this.logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = request.Headers.FirstOrDefault(x => x.Key == CorrelationIdHandler.CorrelationIdHeaderName).Value?.FirstOrDefault();

        var logUrl = AnonymizePersonalIdentifiers(request.RequestUri?.GetLeftPart(UriPartial.Path).ToString());

        var start = DateTime.Now;
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            var time = (int)(DateTime.Now - start).TotalMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(LogMessage, request.Method, logUrl, time, (int)response.StatusCode, response.ReasonPhrase, correlationId);
            }
            else
            {
                logger.LogWarning(LogMessage, request.Method, logUrl, time, (int)response.StatusCode, response.ReasonPhrase, correlationId);
            }

            return response;
        }
        catch (Exception ex)
        {
            var time = (int)(DateTime.Now - start).TotalMilliseconds;
            logger.LogError(ex, LogMessageError, request.Method, logUrl, time, ex.Message, correlationId);
            throw;
        }
    }

    public static string? AnonymizePersonalIdentifiers(string? sourceToAnonymize)
    {
        if (string.IsNullOrWhiteSpace(sourceToAnonymize))
            return sourceToAnonymize;

        return Regex.Replace(sourceToAnonymize, "\\d{11}", "***********", RegexOptions.None, TimeSpan.FromMilliseconds(100));
    }
}
