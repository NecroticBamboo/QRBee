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
        public string AsString() => ClientResponse.AsQRCodeString();

        public static PaymentRequest FromString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ApplicationException("The input is wrong!");
            }

            //doesn't work
            var response = ClientToMerchantResponse.FromString(input);
            return new PaymentRequest(){ClientResponse = response};
        }
    }
}
