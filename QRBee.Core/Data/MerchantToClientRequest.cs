using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Core.Data
{
    public record MerchantToClientRequest
    {
        public string TransactionId
        {
            get;
            init;
        }

        public string Name
        {
            get;
            init;
        }

        public decimal Amount
        {
            get;
            init;
        }

        public DateTime TimeStampUTC
        {
            get;
            init;
        }

        public string? MerchantSignature
        {
            get;
            set;
        }

        public string AsString() => $"{TransactionId}|{Name}|{Amount:0.00}|{TimeStampUTC:O}";

    }
}
