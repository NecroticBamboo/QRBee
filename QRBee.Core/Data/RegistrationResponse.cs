using System;
using System.Collections.Generic;
using System.Text;

namespace QRBee.Core.Data
{
    public record RegistrationResponse
    {
        public string ClientId
        {
            get;
            set;
        }

        public string ClientCertificate
        {
            get;
            set;
        }

        public string APIServerCertificate
        {
            get;
            set;
        }

    }
}
