using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRBee.Core.Data;

namespace QRBee.Load.Generator
{
    internal class TransactionDefiler
    {
        private readonly ILogger<TransactionDefiler> _logger;

        private ThreadSafeRandom _rng = new();

        private double _corruptionProbability;
        private int    _sequenceLengthMin;
        private int    _sequenceLengthMax;
        private int    _corruptionCounter;

        public TransactionDefiler(IOptions<GeneratorSettings> settings, ILogger<TransactionDefiler> logger) 
        {
            _logger = logger;

            var transactionCorruption = settings.Value.TransactionCorruption;
            _corruptionProbability = transactionCorruption.Probability;
            if (_corruptionProbability > 0)
            {
                if (transactionCorruption.Parameters.TryGetValue("SequenceLengthMin", out var s))
                    _sequenceLengthMin = int.Parse(s);
                if (transactionCorruption.Parameters.TryGetValue("SequenceLengthMax", out s))
                    _sequenceLengthMax = int.Parse(s);
                _logger.LogDebug($"Transaction corruption configured: Probability={_corruptionProbability}, SequenceLengthMin={_sequenceLengthMin}, SequenceLengthMax={_sequenceLengthMax}");
            }
        }

        public void CorruptPaymentRequest(PaymentRequest paymentRequest)
        {
            if(Interlocked.Decrement(ref _corruptionCounter) > 0 )
            {
                paymentRequest.ClientResponse.MerchantRequest.Amount += 0.01M;
                return;
            }

            var dice = _rng.NextDouble();
            if (dice < _corruptionProbability)
            {
                paymentRequest.ClientResponse.MerchantRequest.Amount += 10M;
                var cc = _rng.NextInRange(_sequenceLengthMin, _sequenceLengthMax);
                _corruptionCounter = cc;
                _logger.LogWarning($"Anomaly: Corrupted transaction. Dice={dice} SequenceLength={cc}");
            }
        }

        public void CorruptPaymentConfirmation(PaymentConfirmation paymentConfirmation)
        {
            if (Interlocked.Decrement(ref _corruptionCounter) > 0)
            {
                paymentConfirmation.GatewayTransactionId = "BadGatewayTransactionId";
            }
        }
    }
}
