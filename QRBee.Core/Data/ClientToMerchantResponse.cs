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

        public string AsString() => $"{ClientId}|{TimeStampUTC:O}|{Request.AsString()}";

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
