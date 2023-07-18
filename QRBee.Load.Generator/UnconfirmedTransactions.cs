using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QRBee.Load.Generator;

internal class UnconfirmedTransactions
{
    private readonly ILogger<UnconfirmedTransactions> _logger;

    private double           _unconfirmedProbability;
    private TimeSpan         _unconfirmedDuration;
    private DateTime         _anomalyEnd  = DateTime.MinValue;
    private ThreadSafeRandom _rng = new();

    public UnconfirmedTransactions(IOptions<GeneratorSettings> settings, ILogger<UnconfirmedTransactions> logger)
    {
        _logger                 = logger;
        _unconfirmedProbability = settings.Value.UnconfirmedTransaction.Probability;
        _unconfirmedDuration    = TimeSpan.Zero;

        if (_unconfirmedProbability > 0.0)
        {
            if (settings.Value.UnconfirmedTransaction.Parameters.TryGetValue("Duration", out var duration)
                || TimeSpan.TryParse(duration, out _unconfirmedDuration))
            {
                _logger.LogInformation($"Unconfirmed transactions configured. Probablility={_unconfirmedProbability}");
            }
            else
            {
                _unconfirmedProbability = 0.0;
            }
        }
    }

    public bool ShouldConfirm()
    {
        if ( DateTime.UtcNow < _anomalyEnd )
        {
            return false;
        }

        var dice = _rng.NextDouble();
        if (dice < _unconfirmedProbability)
        {
            _anomalyEnd = DateTime.UtcNow + _unconfirmedDuration;
            _logger.LogWarning($"Anomaly: Unconfirmed transaction. Dice={dice} Ends=\"{_anomalyEnd.ToLocalTime():s}\"");
            return false;
        }

        return true;
    }
}
