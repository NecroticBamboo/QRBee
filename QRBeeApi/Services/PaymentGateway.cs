using QRBee.Api.Services.Database;
using QRBee.Core.Data;

namespace QRBee.Api.Services;

internal class PaymentGateway : IPaymentGateway
{
    private readonly ILogger<Storage> _logger;

    public PaymentGateway(ILogger<Storage> logger)
    {
        _logger = logger;
    }

    public Task<GatewayResponse> Payment(TransactionInfo info, ClientCardData clientCardData)
    {
        if (info.Request.ClientResponse.MerchantRequest.Amount > 100)
        {
            _logger.LogInformation($"Transaction with id: {info.Id} failed");
            return Task.FromResult(new GatewayResponse
            {
                Success = false,
                ErrorMessage = "Amount is too high",
                GatewayTransactionId = Guid.NewGuid().ToString()
            });
        }
        _logger.LogInformation($"Transaction with id: {info.Id} succeeded");
        return Task.FromResult(new GatewayResponse
        {
            Success = true,
            GatewayTransactionId = Guid.NewGuid().ToString()
        });
    }

    public Task<GatewayResponse> CancelPayment(TransactionInfo info)
    {
        if (!string.IsNullOrWhiteSpace(info.GatewayTransactionId))
        {
            var m = "Either payment gateway isn't working or the transaction is old";
            _logger.LogError($"Transaction with id: {info.Id} was not cancelled: {m}");
            return Task.FromResult(new GatewayResponse
            {
                Success = false,
                ErrorMessage = m,
                GatewayTransactionId = info.GatewayTransactionId
            });
        }

        _logger.LogInformation($"Transaction with id: {info.Id} was cancelled");
        return Task.FromResult(new GatewayResponse
        {
            Success = true,
            GatewayTransactionId = info.GatewayTransactionId
        });
    }
}