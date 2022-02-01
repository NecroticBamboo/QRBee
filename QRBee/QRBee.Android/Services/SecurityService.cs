using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using QRBee.Core.Security;

namespace QRBee.Api.Services
{
    internal class SecurityService : SecurityServiceBase
    {

        public SecurityService(IPrivateKeyHandler privateKeyHandler)
        : base(privateKeyHandler)
        {
        }

        /// <inheritdoc/>
        public override X509Certificate2 CreateCertificate(string subjectName, byte[] rsaPublicKey)
        {
            throw new ApplicationException("Client never issues certificates");
        }

        /// <inheritdoc/>
        public override X509Certificate2 Deserialize(string pemData)
        {
            throw new NotImplementedException();
            //return X509Certificate2.CreateFromPem(pemData);
        }

        /// <inheritdoc/>
        public override string Serialize(X509Certificate2 cert)
        {
            throw new NotImplementedException();
            // https://stackoverflow.com/questions/43928064/export-private-public-keys-from-x509-certificate-to-pem
            //var pem = PemEncoding.Write("CERTIFICATE", cert.RawData);
            //return new string(pem);
        }
    }

}
