using System.Diagnostics.Metrics;

namespace QRBee.Api.Services
{
    public class CustomMetrics
    {
        private Counter<int>  MerchantRequestCounter               { get; }
        private Counter<int>  MerchantResponseCounter              { get; }
        private Counter<int>  SucceededTransactionsCounter         { get; }
        private Counter<int>  FailedTransactionsCounter            { get; }
        private Counter<int>  CorruptTransactionsCounter           { get; }
        private Counter<int>  CancelledTransactionsCounter         { get; }
        private Counter<int>  SucceededPaymentConfirmationsCounter { get; }
        private Counter<int>  FailedPaymentConfirmationsCounter    { get; }
        private Counter<long> TotalCreditCardCheckTime             { get; }
        private Counter<long> TotalPaymentTime                     { get; }

        private UpDownCounter<int> ConcurrentPayments { get; }
        private UpDownCounter<int> ConcurrentConfirmations { get; }

        public string MetricName { get; }

        public CustomMetrics(string meterName = "QRBeeMetrics") {
            var meter  = new Meter(meterName);
            MetricName = meterName;

            MerchantRequestCounter               = meter.CreateCounter<int>("merchant-requests", description: "Merchant has sent a request");
            MerchantResponseCounter              = meter.CreateCounter<int>("merchant-responses");

            SucceededTransactionsCounter         = meter.CreateCounter<int>("transaction-succeeded", description: "Transaction succeeded");
            FailedTransactionsCounter            = meter.CreateCounter<int>("transaction-failed", description: "Transaction failed");
            CorruptTransactionsCounter           = meter.CreateCounter<int>("transaction-corrupt", description: "Transaction was corrupted");
            CancelledTransactionsCounter         = meter.CreateCounter<int>("transaction-cancelled", description: "Transaction was cancelled by TransactionMonitoring class");

            SucceededPaymentConfirmationsCounter = meter.CreateCounter<int>("payment-confirmation-succeeded", description: "Payment confirmation succeeded");
            FailedPaymentConfirmationsCounter    = meter.CreateCounter<int>("payment-confirmation-failed", description: "Payment confirmation failed");

            TotalCreditCardCheckTime             = meter.CreateCounter<long>("total-credit-card-check-time","msec");
            TotalPaymentTime                     = meter.CreateCounter<long>("total-payment-time","msec");

            ConcurrentPayments                   = meter.CreateUpDownCounter<int>("concurrent-payments");
            ConcurrentConfirmations              = meter.CreateUpDownCounter<int>("concurrent-confirmations");

        }


        public void AddMerchantRequest()                           => MerchantRequestCounter.Add(1);
        public void AddMerchantResponse()                          => MerchantResponseCounter.Add(1);
        public void AddSucceededTransaction()                      => SucceededTransactionsCounter.Add(1);
        public void AddFailedTransaction()                         => FailedTransactionsCounter.Add(1);
        public void AddCorruptTransaction()                        => CorruptTransactionsCounter.Add(1);
        public void AddCancelledTransaction()                      => CancelledTransactionsCounter.Add(1);
        public void AddSucceededPaymentConfirmation()              => SucceededPaymentConfirmationsCounter.Add(1);
        public void AddFailedPaymentConfirmation()                 => FailedPaymentConfirmationsCounter.Add(1);
        public void AddTotalCreditCardCheckTime(long milliseconds) => TotalCreditCardCheckTime.Add(milliseconds);
        public void AddTotalPaymentTime(long milliseconds)         => TotalPaymentTime.Add(milliseconds);

        public void IncreaseConcurrentPayments()                   => ConcurrentPayments.Add(1);
        public void DecreaseConcurrentPayments()                   => ConcurrentPayments.Add(-1);
        public void IncreaseConcurrentConfirmations()              => ConcurrentConfirmations.Add(1);
        public void DecreaseConcurrentConfirmation()               => ConcurrentConfirmations.Add(-1);

    }
}
