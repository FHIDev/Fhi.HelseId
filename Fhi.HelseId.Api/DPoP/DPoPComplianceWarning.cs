using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Api.DPoP;

internal class DPoPComplianceWarning(ILogger<DPoPComplianceWarning> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogWarning("This web API is not configured to require DPoP-tokens. " +
            $"This must be enabled in order to be compliant with the NHN Security profile. " +
            $"It can be enabled by setting the flag {nameof(HelseIdApiKonfigurasjon.RequireDPoPTokens)} " +
            $"to true in appsettings.json under 'HelseIdApiKonfigurasjon'.");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
