namespace QRBee.Core.Data
{
    public record PaymentRequest
    {

        public ClientToMerchantResponse ClientResponse
        {
            get;
            set;
        }

        /// <summary>
        /// Convert PaymentRequest to string
        /// </summary>
        /// <returns>Converted string</returns>
        public string AsString() => $"{ClientResponse.AsDataForSignature()}|{ClientResponse.ClientSignature}";
    }
}
