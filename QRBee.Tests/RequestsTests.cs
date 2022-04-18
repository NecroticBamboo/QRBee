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
    public class RequestsTests
    {
        [TestMethod]
        public void MTC_AsQRCode()
        {
            var mtc = TestData.MakeMTCRequest();
            var s = mtc.AsQRCodeString();

            Assert.AreEqual("555|111-222-333|Merchant|123.45|2022-03-24:20.18.42.5550|merchant-sig", s);
        }

        [TestMethod]
        public void MTC_AsDataForSig()
        {
            var mtc = TestData.MakeMTCRequest();
            var s = mtc.AsDataForSignature();

            Assert.AreEqual("555|111-222-333|Merchant|123.45|2022-03-24:20.18.42.5550", s);
        }

        [TestMethod]
        public void MTC_FromString()
        {
            var mtc = MerchantToClientRequest.FromString("555|111-222-333|Merchant|123.45|2022-03-24:20.18.42.5550|merchant-sig");

            Assert.AreEqual("merchant-sig", mtc.MerchantSignature);
            Assert.AreEqual("555", mtc.MerchantId);
            Assert.AreEqual("Merchant", mtc.Name);
            Assert.AreEqual("111-222-333", mtc.MerchantTransactionId);
            Assert.AreEqual(123.45M, mtc.Amount);

        }

        [TestMethod]
        public void PaymentReq_AsString()
        {
            var pr = TestData.MakePaymentRequest();

            var s = pr.AsString();
            Assert.AreEqual("1234|2022-03-24:20.18.43.1234|555|111-222-333|Merchant|123.45|2022-03-24:20.18.42.5550|merchant-sig|abc", s);
        }

    }
}
