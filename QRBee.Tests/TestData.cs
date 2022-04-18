using QRBee.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Tests
{
    internal class TestData
    {
        public static MerchantToClientRequest MakeMTCRequest()
        {
            return new()
            {
                MerchantId            = "555",
                MerchantTransactionId = "111-222-333",
                Name                  = "Merchant",
                Amount                = 123.45M,
                TimeStampUTC          = DateTime.Parse("2022-03-24 20:18:42.555"),
                MerchantSignature     = "merchant-sig",
            };
        }

        public static ClientToMerchantResponse MakeCTMResponse()
        {
            return new ClientToMerchantResponse
            {
                MerchantRequest         = MakeMTCRequest(),
                ClientId                = "1234",
                TimeStampUTC            = DateTime.Parse("2022-03-24 20:18:43.1234"),
                ClientSignature         = "abc",
                EncryptedClientCardData = "enc-data",
            };
        }

        public static PaymentRequest MakePaymentRequest()
        {
            return new PaymentRequest
            {
                ClientResponse = MakeCTMResponse(),
            };
        }

    }
}
