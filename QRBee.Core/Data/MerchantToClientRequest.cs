using System.Globalization;

namespace QRBee.Core.Data
{
    public record MerchantToClientRequest
    {
        public string MerchantId            { get; set; }
        public string MerchantTransactionId { get; set; }
        public string Name                  { get; set; }
        public decimal Amount               { get; set; }
        public DateTime TimeStampUTC        { get; set; }
        public string MerchantSignature     { get; set; }

        /// <summary>
        /// Convert MerchantToClientRequest to string to be used as QR Code source (along with merchant signature)
        /// </summary>
        /// <returns>String conversion</returns>
        public string AsQRCodeString()     => $"{AsDataForSignature()}|{MerchantSignature}";
        public string AsDataForSignature() => $"{MerchantId}|{MerchantTransactionId}|{Name}|{Amount.ToString("0.00", CultureInfo.InvariantCulture)}|{TimeStampUTC:yyyy-MM-dd:HH.mm.ss.ffff}";

        /// <summary>
        /// Convert from string
        /// </summary>
        /// <param name="input"> A string representation of MerchantToClientRequest</param>
        /// <returns> Converted string</returns>
        /// <exception cref="ApplicationException">Thrown if the input string is incorrect</exception>
        public static MerchantToClientRequest FromString(string input)
        {
            var s = input.Split('|');
            if (s.Length < 6)
            {
                throw new ApplicationException($"Expected 6 elements but got {s.Length}");
            }

            var res = new MerchantToClientRequest
            {
                MerchantId            = s[0],
                MerchantTransactionId = s[1],
                Name                  = s[2],
                Amount                = Convert.ToDecimal(s[3], CultureInfo.InvariantCulture),
                TimeStampUTC          = DateTime.ParseExact(s[4], "yyyy-MM-dd:HH.mm.ss.ffff", null),
                MerchantSignature     = s[5]
            };


            return res;
        }

    }
}
