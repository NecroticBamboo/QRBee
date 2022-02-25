using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IO;
using QRBee.Core.Security;

namespace QRBee.Droid.Services
{
    internal class AndroidSecurityService : SecurityServiceBase
    {
        private X509Certificate2 _apiServerCertificate;
        private string ApiServerCertificateFileName => $"{Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}/ApiServerCertificate.bin";

        public AndroidSecurityService(IPrivateKeyHandler privateKeyHandler)
        : base(privateKeyHandler)
        {
        }

        /// <inheritdoc/>
        public override X509Certificate2 CreateCertificate(string subjectName, byte[] rsaPublicKey)
        {
            throw new ApplicationException("Client never issues certificates");
        }

        private const string CertHeader = "-----BEGIN CERTIFICATE-----";
        private const string CertFooter = "-----END CERTIFICATE-----";

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

        public override X509Certificate2 APIServerCertificate
        {
            get
            {
                if (_apiServerCertificate != null)
                {
                    return _apiServerCertificate;
                }

                if (!File.Exists(ApiServerCertificateFileName))
                    throw new ApplicationException($"File not found: {ApiServerCertificateFileName}");
                var bytes = File.ReadAllBytes(ApiServerCertificateFileName);
                _apiServerCertificate = new X509Certificate2(bytes);
                return _apiServerCertificate;
            }
            set
            {
                _apiServerCertificate = value;
                var bytes = _apiServerCertificate.Export(X509ContentType.Cert);
                File.WriteAllBytes(ApiServerCertificateFileName,bytes);
            }
        }
    }

}
