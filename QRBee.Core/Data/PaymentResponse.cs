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

        /// <summary>
        /// Convert PaymentResponse to string to be encrypted and transmitted back to merchant
        /// </summary>
        /// <returns>Converted string</returns>
        public string AsString() => $"{ServerTransactionId}|{PaymentRequest.AsString()}";
    }
}
