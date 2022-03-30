namespace QRBee.Core.Data
{
    public record ClientToMerchantResponse
    {
        public MerchantToClientRequest MerchantRequest { get; set; }
        public string ClientId                         { get; set; }
        public DateTime TimeStampUTC                   { get; set; }
        public string ClientSignature                  { get; set; }
        public string EncryptedClientCardData          { get; set; }

        /// <summary>
        /// Convert ClientToMerchantResponse to string to be used as QR Code source (along with client signature)
        /// </summary>
        /// <returns> Converted string</returns>
        public string AsQRCodeString()     => $"{ClientId}|{TimeStampUTC:yyyy-MM-dd:HH.mm.ss.ffff}|{ClientSignature}|{EncryptedClientCardData}";
        public string AsDataForSignature() => $"{ClientId}|{TimeStampUTC:yyyy-MM-dd:HH.mm.ss.ffff}|{MerchantRequest.AsQRCodeString()}";

        /// <summary>
        /// Convert from string
        /// </summary>
        /// <param name="input">A string representation of ClientToMerchantResponse</param>
        /// <returns>Converted string</returns>
        /// <exception cref="ApplicationException">Thrown if the input string is incorrect</exception>
        public static ClientToMerchantResponse FromString(string input, MerchantToClientRequest merchantRequest)
        {
            if (merchantRequest == null)
                throw new ArgumentNullException(nameof(merchantRequest));
            if ( string.IsNullOrWhiteSpace(merchantRequest.MerchantSignature) )
                throw new ApplicationException("Request is not signed by a merchant");

            var s = input.Split('|');
            if (s.Length < 4)
            {
                throw new ApplicationException($"Expected 4 elements but got {s.Length}");
            }

            var res = new ClientToMerchantResponse()
            {
                ClientId                = s[0],
                TimeStampUTC            = DateTime.ParseExact(s[1], "yyyy-MM-dd:HH.mm.ss.ffff", null),
                ClientSignature         = s[2],
                EncryptedClientCardData = s[3],
                MerchantRequest         = merchantRequest
            };

            return res;
        }

    }
}
