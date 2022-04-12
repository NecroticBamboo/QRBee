using MongoDB.Bson.Serialization.Attributes;
using QRBee.Core.Data;

namespace QRBee.Api.Services.Database
{
    public record TransactionInfo
    {
        
#pragma warning disable CS8618
        public TransactionInfo()
#pragma warning restore CS8618
        {

        }

        public TransactionInfo(PaymentRequest request, DateTime serverTimeStamp)
        {
            //TODO This is a side effect. The original request will be modified.
            request.ClientResponse.EncryptedClientCardData = null;

            ServerTimeStamp = serverTimeStamp;
            Request = request;
            Id = $"{request.ClientResponse.MerchantRequest.MerchantId}-{request.ClientResponse.MerchantRequest.MerchantTransactionId}";
        }

        /// <summary>
        /// Never use directly. Use <see cref="TransactionId"/> instead.
        /// </summary>
        [BsonId] public string Id { get; set; }

        [BsonIgnore] public string? TransactionId => Id;

        public string? GatewayTransactionId { get; set; }
        public string MerchantTransactionId => Request.ClientResponse.MerchantRequest.MerchantTransactionId;
        public DateTime ServerTimeStamp { get; set; }

        public PaymentRequest Request { get; set; }

        public enum TransactionStatus
        {
            Pending = 0,
            Rejected = 1,
            Succeeded = 2,
            Confirmed = 3,
            Cancelled = 4,
            CancelFailed =5
        }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        public string? RejectReason { get; set; }
    }
}
