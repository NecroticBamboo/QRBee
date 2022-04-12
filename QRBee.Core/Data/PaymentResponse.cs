namespace QRBee.Core.Data
{
    public record PaymentResponse
    {
        public string ServerTransactionId { get; set; }

        public string GatewayTransactionId { get; set; }

        public PaymentRequest PaymentRequest { get; set; }

        public DateTime ServerTimeStampUTC { get; set; }

        public bool Success { get; set; }

        public string RejectReason { get; set; }

        public string ServerSignature { get; set; }

        /// <summary>
        /// Convert PaymentResponse to string to be encrypted and transmitted back to merchant
        /// </summary>
        /// <returns>Converted string</returns>
        public string AsDataForSignature() => $"{ServerTransactionId}|{GatewayTransactionId}|{PaymentRequest.AsString()}|{ServerTimeStampUTC:yyyy-MM-dd:HH.mm.ss.ffff}|{Success}|{RejectReason}";
    }
}
