using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRBee.Core.Data;

namespace QRBee.Load.Generator
{
    internal class TransactionDefiler : AnomalyBase
    {
        public TransactionDefiler(IOptions<GeneratorSettings> settings, ILogger<TransactionDefiler> logger, IAnomalyReporter anomalyReporter) 
            : base("Corrupted transaction", settings.Value.TransactionCorruption, logger, anomalyReporter)
        {
        }

        public void CorruptPaymentRequest(PaymentRequest paymentRequest)
        {
            if ( IsActive() )
                paymentRequest.ClientResponse.MerchantRequest.Amount += 0.01M;
        }

        public void CorruptPaymentConfirmation(PaymentConfirmation paymentConfirmation)
        {
            if (IsActive())
                paymentConfirmation.GatewayTransactionId = "BadGatewayTransactionId";
        }
    }
}
