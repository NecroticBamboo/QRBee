using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QRBee.Load.Generator;

internal class UnconfirmedTransactions : AnomalyBase
{
    public UnconfirmedTransactions(IOptions<GeneratorSettings> settings, ILogger<UnconfirmedTransactions> logger, IAnomalyReporter anomalyReporter)
        : base("Unconfirmed transaction", settings.Value.UnconfirmedTransaction, logger, anomalyReporter)
    {
    }

    public bool ShouldConfirm() => !IsActive();
}
