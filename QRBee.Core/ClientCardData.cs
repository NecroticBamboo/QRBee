namespace QRBee.Core
{
    public class ClientCardData
    {

        public string CardNumber
        {
            get;
            set;
        }

        public string ExpirationDateMMYY
        {
            get;
            set;
        }

        public string ValidFrom
        {
            get;
            set;
        }

        public string CardHolderName
        {
            get;
            set;
        }

        public string CVC
        {
            get;
            set;
        }

        public int? IssueNo
        {
            get;
            set;
        }

        public string AsString() => $"{CardNumber}|{ExpirationDateMMYY}|{ValidFrom}|{CardHolderName}|{CVC}|{IssueNo}";

    }
}
