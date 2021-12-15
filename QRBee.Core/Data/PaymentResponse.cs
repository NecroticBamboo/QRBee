using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Core.Data
{
    public record PaymentResponse
    {

        public string ServerTransactionId
        {
            get;
            init;
        }

        public PaymentRequest PaymentRequest
        {
            get;
            init;
        }

        public string AsString() => $"{ServerTransactionId}|{PaymentRequest.AsString()}";
    }
}
