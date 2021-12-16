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

        public string AsString() => $"{TransactionId}|{Name}|{Amount:0.00}|{TimeStampUTC:O}";

    }
}
