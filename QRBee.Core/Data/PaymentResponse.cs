namespace QRBee.Core.Data
{
    public record PaymentResponse
    {

        public string ServerTransactionId
        {
            get;
            set;
        }

        public PaymentRequest PaymentRequest
        {
            get;
            set;
        }

        public string AsString() => $"{ServerTransactionId}|{PaymentRequest.AsString()}";
    }
}
