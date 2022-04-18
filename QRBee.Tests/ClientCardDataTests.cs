using Microsoft.VisualStudio.TestTools.UnitTesting;
using QRBee.Core.Data;

namespace QRBee.Tests
{
    [TestClass]
    public class ClientCardDataTests
    {
        [TestMethod]
        public void AsString()
        {
            var ccd = new ClientCardData 
            {
                TransactionId        = "1",
                CardNumber           = "2",
                ExpirationDateYYYYMM = "202506",
                ValidFromYYYYMM      = "",
                CardHolderName       = "MR CARDHOLDER",
                CVC                  = "123",
                IssueNo              = 1,
            };

            var s = ccd.AsString();
            Assert.AreEqual( "1|2|202506||MR CARDHOLDER|123|1", s);
        }

        [TestMethod]
        public void FromString()
        {
            var s = "1|2|202506||MR CARDHOLDER|123|";
            var ccd = ClientCardData.FromString(s);

            Assert.AreEqual("1"            , ccd.TransactionId);
            Assert.AreEqual("2"            , ccd.CardNumber);
            Assert.AreEqual("MR CARDHOLDER", ccd.CardHolderName);
            Assert.AreEqual("123"          , ccd.CVC);
            Assert.IsNull(ccd.IssueNo);
        }
    }
}