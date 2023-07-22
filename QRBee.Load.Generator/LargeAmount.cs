using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QRBee.Load.Generator;

internal class LargeAmount : AnomalyBase
{
    private double _minAmount = 1;
    private double _maxAmount = 100;
    private decimal _largeAmountValue = 1000;

    public LargeAmount(IOptions<GeneratorSettings> settings, ILogger<UnconfirmedTransactions> logger, IAnomalyReporter anomalyReporter)
        : base("Large amount", settings.Value.LargeAmount, logger, anomalyReporter)
    {
        _minAmount = settings.Value.MinAmount;
        _maxAmount = settings.Value.MaxAmount;
        if (settings.Value.LargeAmount.Parameters.TryGetValue("Value", out var s))
            _largeAmountValue = decimal.Parse(s);
    }

    public decimal GetAmount()
    {
        if (IsActive())
            return _largeAmountValue;
        return Convert.ToDecimal(_rng.NextDoubleInRange(_minAmount, _maxAmount));
    }
}
