using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QRBee.Load.Generator;

internal class LoadSpike : AnomalyBase
{
    private TimeSpan _spikeDelay;
    private int      _delayBetweenMessagesMSec;
    private int      _delayJitterMSec;

    public LoadSpike(IOptions<GeneratorSettings> settings, ILogger<LoadSpike> logger, IAnomalyReporter anomalyReporter)
        : base("Load spike", settings.Value.LoadSpike, logger, anomalyReporter)
    {
        var loadSpike             = settings.Value.LoadSpike;
        _spikeDelay               = TimeSpan.FromSeconds(15);

        _delayBetweenMessagesMSec = settings.Value.DelayBetweenMessagesMSec;
        _delayJitterMSec          = settings.Value.DelayJitterMSec;

        if (IsEnabled)
        {
            if (loadSpike.Parameters.TryGetValue("Delay", out var duration)
                && TimeSpan.TryParse(duration, out _spikeDelay))
            {
                _spikeDelay = TimeSpan.FromMilliseconds(10);
            }
        }
    }


    public async Task Delay()
    {
        if (IsActive())
            await Task.Delay(_spikeDelay);
        else
            await Task.Delay(_rng.NextInRange(
                _delayBetweenMessagesMSec,
                _delayBetweenMessagesMSec + _delayJitterMSec
                ));
    }
}
