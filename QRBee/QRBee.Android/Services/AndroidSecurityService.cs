using QRBee.Core.Security;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace QRBee.Droid.Services
{
    internal class AndroidSecurityService : SecurityServiceBase
    {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        private X509Certificate2? _apiServerCertificate;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static string ApiServerCertificateFileName => $"{Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}/ApiServerCertificate.bin";

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
