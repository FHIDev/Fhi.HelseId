using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.DPoP;

public class DPoPComplianceWarning(ILogger<DPoPComplianceWarning> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogWarning("This web application is not configured to require DPoP-tokens. " +
            $"This must be enabled in order to be compliant with the NHN Security profile. " +
            $"It can be enabled by setting the flag {nameof(HelseIdWebKonfigurasjon.UseDPoPTokens)} " +
            $"to true in appsettings.json under 'HelseIdWebKonfigurasjon'.");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
