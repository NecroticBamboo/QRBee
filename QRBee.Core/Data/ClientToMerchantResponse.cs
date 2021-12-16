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

        public string AsString() => $"{Request.AsString()}|{ClientId}|{TimeStampUTC:O}";

    }
}
