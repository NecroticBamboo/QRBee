namespace QRBee.Core.Data
{
    public record ClientToMerchantResponse
    {
        public MerchantToClientRequest MerchantRequest
        {
            get;
            set;
        }

        public string ClientId
        {
            get;
            set;
        }

        public DateTime TimeStampUTC
        {
            get;
            set;
        }

        public string ClientSignature
        {
            get;
            set;
        }

        public string EncryptedClientCardData
        {
            get;
            set;
        }

        /// <summary>
        /// Convert ClientToMerchantResponse to string to be used as QR Code source (along with client signature)
        /// </summary>
        /// <returns> Converted string</returns>
        public string AsQRCodeString() => $"{AsDataForSignature()}|{ClientSignature}";

        public string AsDataForSignature() => $"{ClientId}|{TimeStampUTC:O}|{MerchantRequest.AsQRCodeString()}";

        /// <summary>
        /// Convert from string
        /// </summary>
        /// <param name="input">A string representation of ClientToMerchantResponse</param>
        /// <returns>Converted string</returns>
        /// <exception cref="ApplicationException">Thrown if the input string is incorrect</exception>
        public static ClientToMerchantResponse FromString(string input)
        {
            var s = input.Split('|');
            if (s.Length < 3)
            {
                throw new ApplicationException("Expected 3 or more elements");
            }

            var res = new ClientToMerchantResponse()
            {
                MerchantRequest = MerchantToClientRequest.FromString(string.Join("|", s.Skip(2))),
                ClientId = s[0],
                TimeStampUTC = DateTime.ParseExact(s[1], "O", null),
                ClientSignature = s[3]
            };

            return res;
        }

    }
}
