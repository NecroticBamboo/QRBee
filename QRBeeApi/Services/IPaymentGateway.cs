using QRBee.Api.Services.Database;
using QRBee.Core.Data;

namespace QRBee.Api.Services
{

    public class GatewayResponse
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
    }

    public interface IPaymentGateway
    {
        Task<GatewayResponse> Payment(TransactionInfo info, ClientCardData clientCardData);
    }
}
