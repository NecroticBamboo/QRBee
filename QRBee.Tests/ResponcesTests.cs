using Microsoft.VisualStudio.TestTools.UnitTesting;
using QRBee.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Tests
{
    [TestClass]
    public class ResponcesTests
    {
        [TestMethod]
        public void CTMR_AsQRCode()
        {
            ClientToMerchantResponse ctm = TestData.MakeCTMResponse();

            var s = ctm.AsQRCodeString();
            Assert.AreEqual("1234|2022-03-24:20.18.43.1234|abc|enc-data", s);
        }

        [TestMethod]
        public void CTMR_AsDataForSig()
        {
            ClientToMerchantResponse ctm = TestData.MakeCTMResponse();

            var s = ctm.AsDataForSignature();
            Assert.AreEqual("1234|2022-03-24:20.18.43.1234|555|111-222-333|Merchant|123.45|2022-03-24:20.18.42.5550|merchant-sig", s);
        }


        [TestMethod]
        public void CTMR_FromString()
        {
            var mtc = TestData.MakeMTCRequest();
            var ctmr = ClientToMerchantResponse.FromString("1234|2022-03-24:20.18.43.1234|client-sig|client-card", mtc);

            Assert.AreEqual("1234"        , ctmr.ClientId);
            Assert.AreEqual("555"         , ctmr.MerchantRequest.MerchantId);
            Assert.AreEqual("client-sig"  , ctmr.ClientSignature);
            Assert.AreEqual("merchant-sig", ctmr.MerchantRequest.MerchantSignature);
        }

        [TestMethod]
        public void PayResponse_AsDataForSig()
        {
            var pr = new PaymentResponse
            {
                GatewayTransactionId = "gwt-id",
                PaymentRequest       = TestData.MakePaymentRequest(),
                Success              = false,
                RejectReason         = "rejected",
                ServerSignature      = "server-sig",
                ServerTransactionId  = "server_tr-id",
                ServerTimeStampUTC   = DateTime.Parse("2022-03-24 20:18:44.3452"),
            };

            var s= pr.AsDataForSignature();
            Assert.AreEqual("server_tr-id|gwt-id|1234|2022-03-24:20.18.43.1234|555|111-222-333|Merchant|123.45|2022-03-24:20.18.42.5550|merchant-sig|abc|2022-03-24:20.18.44.3452|False|rejected", s);
        }

    }
}
