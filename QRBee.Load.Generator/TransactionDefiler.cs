using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRBee.Core.Data;

namespace QRBee.Load.Generator
{
    internal class TransactionDefiler
    {
        private readonly ILogger<TransactionDefiler> _logger;
        private ThreadSafeRandom _rng = new ThreadSafeRandom();
        private double _corruptionProbability;
        private double _coherentCorruptionProbability;
        private bool _multipleCorruption = false;
        private int _sequenceLengthMin;
        private int _sequenceLengthMax;
        private int _sequenceLength;
        private int _corruptionCounter = 0;

        public TransactionDefiler(IOptions<GeneratorSettings> settings, ILogger<TransactionDefiler> logger) 
        {
            _logger = logger;

            var transactionCorruption = settings.Value.TransactionCorruption;
            _corruptionProbability = transactionCorruption.Probability;

            var coherentTransactionCorruption = settings.Value.CoherentTransactionCorruption;
            _coherentCorruptionProbability = coherentTransactionCorruption.Probability;
            if (_coherentCorruptionProbability > 0)
            {
                if (coherentTransactionCorruption.Parameters.TryGetValue("SequenceLengthMin", out var s))
                    _sequenceLengthMin = int.Parse(s);
                if (coherentTransactionCorruption.Parameters.TryGetValue("SequenceLengthMax", out s))
                    _sequenceLengthMax = int.Parse(s);
            }


            _logger.LogDebug($"Transaction corruption configured: Probability={_corruptionProbability}");
            _logger.LogDebug($"Coherent transaction corruption configured: Probability={_coherentCorruptionProbability}, SequenceLengthMin={_sequenceLengthMin}, SequenceLengthMax={_sequenceLengthMax}");
        }

        public void CorruptPaymentRequest(PaymentRequest paymentRequest)
        {
            var dice = _rng.NextDouble();
            if(_multipleCorruption)
            {
                _corruptionCounter++;
                _logger.LogWarning($"Anomaly: Coherent corrupted transaction Dice={dice}, Corruption counter = {_corruptionCounter}, Sequence length = {_sequenceLength}");
                paymentRequest.ClientResponse.MerchantRequest.Amount += 0.01M;

                if (_corruptionCounter >= _sequenceLength)
                {
                    _corruptionCounter = 0;
                    _multipleCorruption = false;
                }
                return;
            }
            
            if (dice < _corruptionProbability)
            {
                _logger.LogWarning($"Anomaly: Corrupted transaction Dice={dice}");
                paymentRequest.ClientResponse.MerchantRequest.Amount += 10M;

                if(dice < _coherentCorruptionProbability && _multipleCorruption==false)
                {
                    _sequenceLength = _rng.NextInRange(_sequenceLengthMin,_sequenceLengthMax);
                    _multipleCorruption = true;
                }
                
            }
            
        }

        public void CorruptPaymentConfirmation(PaymentConfirmation paymentConfirmation)
        {
            var dice = _rng.NextDouble();
            if (dice < _corruptionProbability)
            {
                _logger.LogWarning($"Anomaly: Corrupted transaction confirmation Dice={dice}");
                paymentConfirmation.GatewayTransactionId = "BadGatewayTransactionId";
            }

        }
    }
}
