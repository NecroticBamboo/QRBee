using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Core.Data
{
    public record ClientToMerchantResponse
    {
        public MerchantToClientRequest Request
        {
            get;
            init;
        }

        public string ClientId
        {
            get;
            init;
        }

        public DateTime TimeStampUTC
        {
            get;
            init;
        }

        public string? ClientSignature
        {
            get;
            set;
        }

        public string EncryptedClientCardData
        {
            get;
            init;
        }

        public string AsString() => $"{Request.AsString()}|{ClientId}|{TimeStampUTC:O}";

    }
}
