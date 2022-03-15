using QRBee.Core.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace QRBee.Api.Services
{
    internal class SecurityService : SecurityServiceBase
    {

        private const int CertificateValidityPeriodDays = 365;
        private const string CertHeader = "-----BEGIN CERTIFICATE-----";
        private const string CertFooter = "-----END CERTIFICATE-----";

        public SecurityService(IPrivateKeyHandler privateKeyHandler)
        : base(privateKeyHandler)
        {
        }

        /// <inheritdoc/>
        public override X509Certificate2 CreateCertificate(string subjectName, byte[] rsaPublicKey)
        {
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportRSAPublicKey(rsaPublicKey, out _);

            var distinguishedName = new X500DistinguishedName($"CN={subjectName}");
            var req = CreateClientCertRequest(distinguishedName, rsa);

            var pk = PrivateKeyHandler.LoadPrivateKey();
            var cert = PrivateKeyHandler.GetCertificate();
            var newCert = cert.CopyWithPrivateKey(pk);

            var clientCert = req.Create(newCert,
                DateTimeOffset.UtcNow - TimeSpan.FromDays(1),
                DateTimeOffset.UtcNow + TimeSpan.FromDays(CertificateValidityPeriodDays),
                Guid.NewGuid()
                    .ToByteArray());

            return clientCert;
        }

        /// <summary>
        /// Generate Client certificate request (i.e. without KeyCertSign usage extension)
        /// </summary>
        /// <param name="distinguishedName"></param>
        /// <param name="rsa"></param>
        /// <returns></returns>
        private static CertificateRequest CreateClientCertRequest(X500DistinguishedName distinguishedName, RSA rsa)
        {
            var request = new CertificateRequest(
                distinguishedName,
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DataEncipherment
                    | X509KeyUsageFlags.KeyEncipherment
                    | X509KeyUsageFlags.DigitalSignature,
                    false));

            return request;
        }

        /// <inheritdoc/>
        public override X509Certificate2 Deserialize(string pemData)
        {
            var start = pemData.IndexOf(CertHeader);
            if (start == -1)
                throw new ApplicationException("Invalid certificate format");
            start = start + CertHeader.Length;

            var end = pemData.IndexOf(CertFooter);
            if (end == -1)
                throw new ApplicationException("Invalid certificate format");

            var base64 = pemData.Substring(start, end - start); // contains new lines, but it does not matter
            var data = Convert.FromBase64String(base64);
            
            return new X509Certificate2(data);
        }

        /// <inheritdoc/>
        public override string Serialize(X509Certificate2 cert)
        {
            // https://stackoverflow.com/questions/43928064/export-private-public-keys-from-x509-certificate-to-pem
            var builder = new StringBuilder();
            builder.AppendLine(CertHeader);
            builder.AppendLine(Convert.ToBase64String(cert.RawData, Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine(CertFooter);

            return builder.ToString();
        }

        /// <inheritdoc/>
        public override X509Certificate2 APIServerCertificate
        {
            get => PrivateKeyHandler.GetCertificate();
            set => throw new ApplicationException("Do not call this");
        }
    }

}
