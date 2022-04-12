namespace QRBee.Core.Data
{
    public class PaymentConfirmation
    {
        public string MerchantId { get; set; }

        public string MerchantTransactionId { get; set; }

        public string GatewayTransactionId { get; set; }
    }
}
