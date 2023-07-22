using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRBee.Core.Data;
using System.Text;

namespace QRBee.Load.Generator;

internal class PaymentRequestGenerator
{
    private readonly ClientPool _clientPool;
    private readonly ILogger<PaymentRequestGenerator> _logger;
    private readonly LargeAmount _largeAmount;

    public PaymentRequestGenerator(ClientPool clientPool, IOptions<GeneratorSettings> settings, ILogger<PaymentRequestGenerator> logger, LargeAmount largeAmount)
    {
        _clientPool = clientPool;
        _logger     = logger;
        _largeAmount = largeAmount;
    }

    public async Task<PaymentRequest> GeneratePaymentRequest(int clientId, int merchantId)
    {
        var merchant = await GetMerchant(merchantId);
        var merchantReq = new MerchantToClientRequest()
        {
            MerchantId            = merchant.ClientId,
            MerchantTransactionId = Guid.NewGuid().ToString(),
            Name                  = merchant.CardHolderName,
            Amount                = GetAmount(),
            TimeStampUTC          = DateTime.UtcNow
        };

        var merchantSignature = merchant.SecurityService.Sign(Encoding.UTF8.GetBytes(merchantReq.AsDataForSignature()));
        merchantReq.MerchantSignature = Convert.ToBase64String(merchantSignature);

        var clientResp = await CreateClientResponse(merchantReq, clientId);

        var req = new PaymentRequest()
        {
            ClientResponse = clientResp
        };

        return req;
    }

    private async Task<ClientToMerchantResponse> CreateClientResponse(MerchantToClientRequest merchantReq, int clientId)
    {
        var client = await GetClient(clientId);

        var response = new ClientToMerchantResponse
        {
            ClientId                = client.ClientId,
            TimeStampUTC            = DateTime.UtcNow,
            MerchantRequest         = merchantReq,
            EncryptedClientCardData = EncryptCardData(client, merchantReq.MerchantTransactionId)
        };

        var clientSignature = client.SecurityService.Sign(Encoding.UTF8.GetBytes(response.AsDataForSignature()));
        response.ClientSignature = Convert.ToBase64String(clientSignature);

        return response;
    }

    private decimal GetAmount() => _largeAmount.GetAmount();

    private Task<ClientSettings> GetMerchant(int id) => _clientPool.GetMerchant(id);

    private Task<ClientSettings> GetClient(int id) => _clientPool.GetClient(id);

    private string EncryptCardData(ClientSettings settings, string transactionId)
    {
        var clientCardData = new ClientCardData
        {
            TransactionId        = transactionId,
            CardNumber           = settings.CardNumber,
            ExpirationDateYYYYMM = string.IsNullOrWhiteSpace(settings.ExpirationDate) ? null : DateTime.Parse(settings.ExpirationDate).ToString("yyyy-MM"),
            ValidFromYYYYMM      = string.IsNullOrWhiteSpace(settings.ValidFrom) ? null : DateTime.Parse(settings.ValidFrom).ToString("yyyy-MM"),
            CardHolderName       = settings.CardHolderName,
            CVC                  = settings.CVC,
            IssueNo              = settings.IssueNo
        };

        var bytes = settings.SecurityService.Encrypt(Encoding.UTF8.GetBytes(clientCardData.AsString()), settings.SecurityService.APIServerCertificate);
        return Convert.ToBase64String(bytes);
    }

}
