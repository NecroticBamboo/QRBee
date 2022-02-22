using System;
using System.Collections.Generic;
using System.Text;

namespace QRBee.Core.Security
{
    public class ReadableCertificateRequest
    {
        private byte[] _signature;

        public string SubjectName { get; set; }

        public StringRSAParameters RsaPublicKey { get; set; }

        public string Signature
        {
            get => _signature != null ? Convert.ToBase64String(_signature): null;
            set => _signature = value != null ? Convert.FromBase64String(value) : null;
        }

        public string AsDataForSignature() => $"{SubjectName}|{RsaPublicKey.ConvertToJson()}";

    }
}
