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
        if (info.Request.ClientResponse.MerchantRequest.Amount < 10)
        {
            _logger.LogInformation($"Transaction with id: {info.Id} failed");
            return Task.FromResult(new GatewayResponse
            {
                Success = false,
                ErrorMessage = "Amount is too low"
            });
        }
        _logger.LogInformation($"Transaction with id: {info.Id} succeeded");
        return Task.FromResult(new GatewayResponse
        {
            Success = true
        });
    }
}