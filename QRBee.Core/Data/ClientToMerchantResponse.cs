namespace QRBee.Core.Data
{
    public record ClientToMerchantResponse
    {
        public MerchantToClientRequest Request
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
        public string AsString() => $"{ClientId}|{TimeStampUTC:O}|{Request.AsString()}";

        /// <summary>
        /// Convert from string
        /// </summary>
        /// <param name="input">A string representation of ClientToMerchantResponse</param>
        /// <returns>Converted string</returns>
        /// <exception cref="ApplicationException">Thrown if the input string is incorrect</exception>
        public static ClientToMerchantResponse FromString(string input)
        {
            var s = input.Split('|');
            if (s.Length != 3)
            {
                throw new ApplicationException("Expected 3 elements");
            }

            var res = new ClientToMerchantResponse()
            {
                Request = MerchantToClientRequest.FromString(string.Join("|", s.Skip(2))),
                ClientId = s[0],
                TimeStampUTC = DateTime.ParseExact(s[1], "O", null)
            };

            return res;
        }

    }
}
