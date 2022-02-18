﻿using MongoDB.Bson;
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
        public DateTime ServerTimeStamp { get; set; }

        public PaymentRequest Request { get; set; }

    }
}