using log4net.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Load.Generator
{
    internal class LoadSpike
    {
        private readonly IOptions<GeneratorSettings> _settings;
        private readonly ILogger<LoadSpike>          _logger;

        private TimeSpan         _spikeDuration;
        private TimeSpan         _spikeDelay;
        private double           _spikeProbability;
        private bool             _spikeActive;

        private ThreadSafeRandom _rng = new();
        private DateTime         _spikeEnd = DateTime.MinValue;

        public LoadSpike( IOptions<GeneratorSettings> settings, ILogger<LoadSpike> logger )
        {
            _settings         = settings;
            _logger           = logger;

            var loadSpike     = settings.Value.LoadSpike;
            _spikeDuration    = TimeSpan.Zero;
            _spikeDelay       = TimeSpan.Zero;
            _spikeProbability = loadSpike?.Probability ?? 0.0;

            if (loadSpike != null && loadSpike.Probability > 0.0)
            {
                if (!loadSpike.Parameters.TryGetValue("Duration", out var duration)
                    || !TimeSpan.TryParse(duration, out _spikeDuration))
                {
                    _spikeProbability = 0.0;
                }
                else
                {
                    if (loadSpike.Parameters.TryGetValue("Delay", out duration)
                        && TimeSpan.TryParse(duration, out _spikeDelay))
                    {
                        _spikeDelay = TimeSpan.FromMilliseconds(10);
                        _logger.LogDebug($"Load spike configured. Probablility={_spikeProbability} Duration=\"{_spikeDuration:g}\" Delay=\"{_spikeDelay:g}\"");
                    }
                }
            }

        }

        public async Task Delay()
        {
            if (DateTime.Now > _spikeEnd)
            {
                if (_spikeActive)
                {
                    _spikeActive = false;
                    _logger.LogWarning($"Anomaly: Load spike ended");
                }

                var dice = _rng.NextDouble();
                if (dice < _spikeProbability)
                {
                    // start load spike
                    _spikeEnd = DateTime.Now + _spikeDuration;
                    _spikeActive = true;

                    _logger.LogWarning($"Anomaly: Load spike until {_spikeEnd} Dice={dice}");

                    await Task.Delay(_spikeDelay);
                }
                else
                {
                    await Task.Delay(_rng.NextInRange(
                        _settings.Value.DelayBetweenMessagesMSec,
                        _settings.Value.DelayBetweenMessagesMSec + _settings.Value.DelayJitterMSec
                        ));
                }
            }
            else
            {
                await Task.Delay(_spikeDelay);
            }
        }

    }
}
