namespace QRBee.Core.Data
{
    public record PaymentRequest
    {

        public ClientToMerchantResponse Request
        {
            get;
            set;
        }

        public string AsString() => Request.AsString();

    }
}
