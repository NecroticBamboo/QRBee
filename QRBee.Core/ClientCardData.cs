using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Core
{
    public class ClientCardData
    {

        public string CardNumber
        {
            get;
            init;
        }

        public string ExpirationDateMMYY
        {
            get;
            init;
        }

        public string? ValidFrom
        {
            get;
            set;
        }

        public string CardHolderName
        {
            get;
            init;
        }

        public string CVC
        {
            get;
            init;
        }

        public int? IssueNo
        {
            get;
            set;
        }

        public string AsString() => $"{CardNumber}|{ExpirationDateMMYY}|{ValidFrom}|{CardHolderName}|{CVC}|{IssueNo}";

    }
}
