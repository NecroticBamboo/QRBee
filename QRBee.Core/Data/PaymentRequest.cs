using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Core.Data
{
    public record PaymentRequest
    {

        public ClientToMerchantResponse Request
        {
            get;
            init;
        }

        public string AsString() => Request.AsString();

    }
}
