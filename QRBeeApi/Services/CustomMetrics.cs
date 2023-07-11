using System.Diagnostics.Metrics;

namespace QRBee.Api.Services
{
    public class CustomMetrics
    {
        private Counter<int> MerchantRequestCounter { get; }
        private Counter<int> MerchantResponseCounter { get; }
        private Counter<int> SucceededTransactionsCounter { get; }
        private Counter<int> FailedTransactionsCounter { get; }
        private Counter<long> TotalCreditCardCheckTime { get; }
        private Counter<long> TotalPaymentTime { get; }
        

        public string MetricName { get; }

        public CustomMetrics(string meterName = "QRBeeMetrics") {
            var meter = new Meter(meterName);
            MetricName = meterName;

            MerchantRequestCounter = meter.CreateCounter<int>("merchant-requests", description: "Merchant has sent a request");
            MerchantResponseCounter = meter.CreateCounter<int>("merchant-responses");
            SucceededTransactionsCounter = meter.CreateCounter<int>("transaction-succeeded", description: "Transaction succeeded");
            FailedTransactionsCounter = meter.CreateCounter<int>("transaction-failed", description: "Transaction failed");

            TotalCreditCardCheckTime = meter.CreateCounter<long>("Total-credit-card-check-time","msec");
            TotalPaymentTime = meter.CreateCounter<long>("Total-payment-time","msec");
        }


        public void AddMerchantRequest() => MerchantRequestCounter.Add(1);
        public void AddMerchantResponse() => MerchantResponseCounter.Add(1);
        public void AddSucceededTransaction() => SucceededTransactionsCounter.Add(1);
        public void AddFailedTransaction() => FailedTransactionsCounter.Add(1);
        public void AddTotalCreditCardCheckTime(long milliseconds) => TotalCreditCardCheckTime.Add(milliseconds);
        public void AddTotalPaymentTime(long milliseconds) => TotalPaymentTime.Add(milliseconds);

    }
}
