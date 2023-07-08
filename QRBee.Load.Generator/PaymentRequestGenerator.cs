using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRBee.Core.Data;
using System.Text;

namespace QRBee.Load.Generator;

internal class PaymentRequestGenerator
{
    private readonly ClientPool _clientPool;
    private readonly ILogger<PaymentRequestGenerator> _logger;
    private ThreadSafeRandom _rng       = new ThreadSafeRandom();
    private double _minAmount = 1;
    private double _maxAmount = 100;
    private double _largeAmountProbability;
    private double _largeAmountValue;

    public PaymentRequestGenerator(ClientPool clientPool, IOptions<GeneratorSettings> settings, ILogger<PaymentRequestGenerator> logger)
    {
        _clientPool = clientPool;
        _logger     = logger;
        _minAmount  = settings.Value.MinAmount;
        _maxAmount  = settings.Value.MaxAmount;

        var largeAmount = settings.Value.LargeAmount;
        _largeAmountProbability = largeAmount.Probability;
        if (_largeAmountProbability > 0)
        {
            if ( largeAmount.Parameters.TryGetValue("Value", out var s))
                _largeAmountValue = Double.Parse(s);
        }

        if (_largeAmountValue <= 0.0)
            _largeAmountProbability = 0.0;
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

    private decimal GetAmount()
    {
        if (_rng.NextDouble() < _largeAmountProbability)
        {
            _logger.LogWarning($"Anomaly: Large amount");
            return Convert.ToDecimal(_rng.NextDoubleInRange(_largeAmountValue, _largeAmountValue* 1.10));
        }
        return Convert.ToDecimal(_rng.NextDoubleInRange(_minAmount, _maxAmount));
    }

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
