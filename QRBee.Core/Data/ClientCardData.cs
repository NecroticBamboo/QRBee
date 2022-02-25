namespace QRBee.Core.Data
{
    public class ClientCardData
    {
        public string TransactionId      { get; set; }
        public string CardNumber         { get; set; }
        public string ExpirationDateMMYY { get; set; }
        public string ValidFrom          { get; set; }
        public string CardHolderName     { get; set; }
        public string CVC                { get; set; }
        public int? IssueNo              { get; set; }

        /// <summary>
        /// Convert ClientCardData to string to be used as a source for encryption.
        /// WARNING: this should always be encrypted and never transmitted in clear text form.
        /// </summary>
        /// <returns>Converted string</returns>
        public string AsString() => $"{TransactionId}|{CardNumber}|{ExpirationDateMMYY}|{ValidFrom}|{CardHolderName}|{CVC}|{IssueNo}";

    }
}
