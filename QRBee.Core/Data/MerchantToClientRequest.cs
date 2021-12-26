using System.Globalization;

namespace QRBee.Core.Data
{
    public record MerchantToClientRequest
    {
        public string TransactionId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public decimal Amount
        {
            get;
            set;
        }

        public DateTime TimeStampUTC
        {
            get;
            set;
        }

        public string MerchantSignature
        {
            get;
            set;
        }

        public string AsString() => $"{TransactionId}|{Name}|{Amount.ToString("0.00", CultureInfo.InvariantCulture)}|{TimeStampUTC:O}";

        public static MerchantToClientRequest FromString(string input)
        {
            var s = input.Split('|');
            if (s.Length != 4)
            {
                throw new ApplicationException("Expected 4 elements");
            }

            var res = new MerchantToClientRequest
            {
                TransactionId = s[0],
                Name = s[1],
                Amount = Convert.ToDecimal(s[2], CultureInfo.InvariantCulture),
                TimeStampUTC = DateTime.ParseExact(s[3],"O",null)
            };


            return res;
        }

    }
}
