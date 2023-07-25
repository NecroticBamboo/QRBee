using Microsoft.Extensions.Logging;

namespace QRBee.Load.Generator;

internal class AnomalyBase
{
    private readonly ILogger          _logger;
    private readonly IAnomalyReporter _anomalyReporter;
    protected ThreadSafeRandom        _rng          = new();
    private bool                      _anomalyActive;
    private DateTime                  _anomalyStart = DateTime.MinValue;
    private DateTime                  _anomalyEnd   = DateTime.MinValue;
    private double                    _anomalyProbability;
    private TimeSpan                  _anomalyDuration;
    private readonly object           _lock         = new();

    public string Name { get; }

    public AnomalyBase(string name, Anomaly settings, ILogger logger, IAnomalyReporter anomalyReporter)
    {
        Name                = name;
        _logger             = logger;
        _anomalyReporter    = anomalyReporter;

        _anomalyProbability = settings.Probability;
        _anomalyDuration    = settings.Duration;
        if (IsEnabled)
            _logger.LogDebug($"{Name} configured: Probability={_anomalyProbability}, Duration={_anomalyDuration}");
    }

    public bool IsEnabled => _anomalyProbability > 0.0;

    protected bool IsActive()
    {
        if (DateTime.UtcNow < _anomalyEnd)
        {
            return true;
        }
        else if (_anomalyActive)
        {
            // double locking
            lock (_lock)
            {
                if (_anomalyActive)
                {
                    _anomalyActive = false;
                    // _anomalyReporter.Report(_anomalyStart, _anomalyEnd, "Unconfirmed transaction");
                    _logger.LogWarning($"Anomaly:{Name} ended");
                }
            }
        }

        var dice = _rng.NextDouble();
        if (dice < _anomalyProbability)
        {
            lock (_lock)
            {
                if (!_anomalyActive)
                {
                    _anomalyStart  = DateTime.Now;
                    _anomalyEnd    = _anomalyStart + _anomalyDuration;
                    _anomalyActive = true;
                    _logger.LogWarning($"Anomaly: {Name}. Dice={dice} Ends=\"{_anomalyEnd.ToLocalTime():s}\"");
                    _anomalyReporter.Report(_anomalyStart, _anomalyEnd, $"{Name}");
                }
            }
            return true;
        }

        return false;
    }
}
