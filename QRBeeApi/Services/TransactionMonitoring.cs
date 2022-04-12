using QRBee.Api.Services.Database;

namespace QRBee.Api.Services
{
    public class TransactionMonitoring
    {
        private readonly IStorage _storage;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ILogger<TransactionMonitoring> _logger;
        private const double Minutes = 5;

        private static bool _started;
        private static object _syncObject = new();

        public TransactionMonitoring(IStorage storage, IPaymentGateway paymentGateway, ILogger<TransactionMonitoring> logger)
        {
            _storage = storage;
            _paymentGateway = paymentGateway;
            _logger = logger;

            if (_started) 
                return;

            lock (_syncObject)
            {
                if (_started) 
                    return;

                Task.Run(MonitoringLoop);
                _started = true;
            }

        }
        private async Task MonitoringLoop()
        {
            _logger.LogInformation("Starting monitoring loop");
            while (true)
            {
                await CheckTransactions();
                await Task.Delay(TimeSpan.FromMinutes(Minutes));
            }
        }

        private async Task CheckTransactions()
        {
            var list = await _storage.GetTransactionsByStatus(TransactionInfo.TransactionStatus.Succeeded);
            _logger.LogDebug($"Found {list.Count} unconfirmed transactions");

            foreach (var transaction in list)
            {
                if (transaction.ServerTimeStamp + TimeSpan.FromMinutes(Minutes) > DateTime.UtcNow)
                {
                    _logger.LogDebug($"Transaction: {transaction.MerchantTransactionId} should not be cancelled yet (ServerTimeStamp: {transaction.ServerTimeStamp:O}, Now: {DateTime.UtcNow:O})");
                    continue;
                }

                _logger.LogDebug($"Cancelling transaction: {transaction.MerchantTransactionId}...");
                await CancelTransaction(transaction);
            }
        }

        private async Task CancelTransaction(TransactionInfo transaction)
        {
            try
            {
                await _paymentGateway.CancelPayment(transaction);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Transaction: {transaction.MerchantTransactionId} can't be cancelled: {e.Message}");
                transaction.Status = TransactionInfo.TransactionStatus.CancelFailed;
                await _storage.UpdateTransaction(transaction);
                return;
            }

            transaction.Status = TransactionInfo.TransactionStatus.Cancelled;
            await _storage.UpdateTransaction(transaction);
        }
    }
}
