using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace QRBee.Core.Security
{
    /// <summary>
    /// Private key handler for API server
    /// </summary>
    public class PrivateKeyHandlerBase : IPrivateKeyHandler
    {

        protected string CommonName { get; set; }
        protected string CertificatePassword { get; set; }
        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public string GeneratePrivateKey(string? subjectName = null)
        {
            throw new NotImplementedException();
        }

        public string CreateCertificateRequest()
        {
            throw new NotImplementedException();
        }

        public void AttachCertificate(X509Certificate2 cert)
        {
            throw new NotImplementedException();
        }

        public X509Certificate2 LoadPrivateKey()
        {
            throw new NotImplementedException();
        }
    }
}
