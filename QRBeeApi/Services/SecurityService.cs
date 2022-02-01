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
            if (!IsValidSubjectName(subjectName))
                throw new CryptographicException("Invalid subject name");

            // https://stackoverflow.com/questions/60930065/generate-and-sign-certificate-in-different-machines-c-sharp

            using var publicKey = RSA.Create();

            publicKey.ImportSubjectPublicKeyInfo(rsaPublicKey, out var nBytes);
            //TODO: check that nBytes is within allowed range

            var request = new CertificateRequest("CN=" + subjectName, publicKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.NonRepudiation, false));
            request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

            // create a new certificate
            using var caPrivateKey = PrivateKeyHandler.LoadPrivateKey();
            var certificate = request.Create(
                caPrivateKey,
                DateTimeOffset.UtcNow.AddSeconds(-1), // user can use it now
                DateTimeOffset.UtcNow.AddDays(30), // user need to login every 30 days
                Guid.NewGuid().ToByteArray());

            return certificate;

        }

        /// <inheritdoc/>
        public override X509Certificate2 Deserialize(string pemData)
        {
            return X509Certificate2.CreateFromPem(pemData);
        }

        /// <inheritdoc/>
        public override string Serialize(X509Certificate2 cert)
        {
            // https://stackoverflow.com/questions/43928064/export-private-public-keys-from-x509-certificate-to-pem
            var pem = PemEncoding.Write("CERTIFICATE", cert.RawData);
            return new string(pem);
        }
    }

}
