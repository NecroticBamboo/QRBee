namespace QRBee.Core.Data
{
    public record PaymentRequest
    {

        public ClientToMerchantResponse Request
        {
            get;
            set;
        }

        /// <summary>
        /// Convert PaymentRequest to string
        /// </summary>
        /// <returns>Converted string</returns>
        public string AsString() => Request.AsString();

    }
}
