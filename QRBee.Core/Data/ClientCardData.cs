namespace QRBee.Core.Data
{
    public class ClientCardData
    {
        public string TransactionId      { get; set; }
        public string CardNumber         { get; set; }
        public string ExpirationDateYYYYMM { get; set; }
        public string ValidFromYYYYMM      { get; set; }
        public string CardHolderName     { get; set; }
        public string CVC                { get; set; }
        public int? IssueNo              { get; set; }

        /// <summary>
        /// Convert ClientCardData to string to be used as a source for encryption.
        /// WARNING: this should always be encrypted and never transmitted in clear text form.
        /// </summary>
        /// <returns>Converted string</returns>
        public string AsString() => $"{TransactionId}|{CardNumber}|{ExpirationDateYYYYMM}|{ValidFromYYYYMM}|{CardHolderName}|{CVC}|{IssueNo ?? 0}";

        public static ClientCardData FromString(string input)
        {
            var s = input.Split('|');
            if (s.Length < 7)
            {
                throw new ApplicationException("Expected 7 or more elements");
            }

            var res = new ClientCardData()
            {
                TransactionId      = s[0],
                CardNumber         = s[1],
                ExpirationDateYYYYMM = s[2],
                ValidFromYYYYMM      = s[3],
                CardHolderName     = s[4],
                CVC                = s[5]
            };

            if (!string.IsNullOrWhiteSpace(s[6]))
                res.IssueNo = Convert.ToInt32(s[6]);

            if (res.IssueNo <= 0)
            {
                res.IssueNo = null;
            }

            return res;
        }

    }
}
