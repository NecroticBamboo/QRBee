using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using QRBee.Core.Security;

namespace QRBee.Api.Services
{
    /// <summary>
    /// Private key handler for API server
    /// </summary>
    public class ServerPrivateKeyHandler : IPrivateKeyHandler
    {
        private X509Certificate2? _certificate;
        private readonly object   _syncObject = new object();

        private const string FileName             = "private_key.p12";
        private const int RSABits                 = 2048;
        private const int CertificateValidityDays = 3650;

        private const string VeryBadNeverUseCertificatePassword = "+ñèbòFëc×ŽßRúß¿ãçPN";

        private string PrivateKeyFileName => $"{System.Environment.SpecialFolder.LocalApplicationData}/{FileName}";

        /// <inheritdoc/>
        public bool Exists()
            => File.Exists(PrivateKeyFileName);

        /// <inheritdoc/>
        public ReadableCertificateRequest GeneratePrivateKey(string subjectName)
        {
            // locking used to make sure that only one thread generating a private key
            lock (_syncObject)
            {
                var pk = CreateSelfSignedServerCertificate(subjectName);
                var pkcs12data = pk.Export(X509ContentType.Pfx, VeryBadNeverUseCertificatePassword);
                File.WriteAllBytes(PrivateKeyFileName, pkcs12data);

                _certificate?.Dispose();
                _certificate = new X509Certificate2(pkcs12data, VeryBadNeverUseCertificatePassword);
            }

            return CreateCertificateRequest(subjectName);
        }

        /// <inheritdoc/>
        public ReadableCertificateRequest CreateCertificateRequest(string subjectName)
        {
            //TODO in fact server should create certificate request in standard format if we ever want to get externally sighed certificate.
            var pk = LoadPrivateKey();
            var rsa = pk.GetRSAPublicKey();

            if (rsa == null)
            {
                throw new ApplicationException("Object missing public key.");
            }

            var request = new ReadableCertificateRequest
            {
                RsaPublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
                SubjectName = subjectName
            };
            var data = Encoding.UTF8.GetBytes(request.AsDataForSignature());

            //We can't use SecurityService here because it uses this class. This creates cyclic dependency.
            var signature = rsa?.SignData(data, 0, data.Length, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ?? throw new CryptographicException("No private key found");
            request.Signature = Convert.ToBase64String(signature);
            return request;
        }

        //private static string AsCsr(CertificateRequest request)
        //{
        //    // https://stackoverflow.com/questions/65943968/how-to-convert-a-csr-text-file-into-net-core-standard-certificaterequest-for-s

        //    var encoded = request.CreateSigningRequest();
        //    var payload = Convert.ToBase64String(encoded, Base64FormattingOptions.InsertLineBreaks);
        //    using var stream = new MemoryStream();
        //    using (var writer = new StreamWriter(stream, System.Text.Encoding.UTF8, 512, true))
        //    {
        //        writer.WriteLine("-----BEGIN CERTIFICATE REQUEST-----");
        //        writer.WriteLine(payload);
        //        writer.WriteLine("-----END CERTIFICATE REQUEST-----");
        //        writer.Flush();
        //    }

        //    stream.Position = 0;
        //    using (var reader = new StreamReader(stream))
        //    {
        //        return reader.ReadToEnd();
        //    }
        //}

        /// <summary>
        /// Generate EXPORTABLE certificate
        /// </summary>
        /// <param name="subjectName"></param>
        /// <returns></returns>
        private X509Certificate2 CreateSelfSignedServerCertificate(string subjectName)
        {
            // https://stackoverflow.com/questions/42786986/how-to-create-a-valid-self-signed-x509certificate2-programmatically-not-loadin

            var distinguishedName = new X500DistinguishedName($"CN={subjectName}");

            using RSA rsa = RSA.Create(RSABits);
            var request = CreateClientCertificateRequest(distinguishedName, rsa);

            var certificate = request.CreateSelfSigned(
                new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
                new DateTimeOffset(DateTime.UtcNow.AddDays(CertificateValidityDays))
            );

            return certificate;
        }

        /// <summary>
        /// Generate CA certificate request (i.e. with KeyCertSign usage extension)
        /// </summary>
        /// <param name="distinguishedName"></param>
        /// <param name="rsa"></param>
        /// <returns></returns>
        private static CertificateRequest CreateClientCertificateRequest(X500DistinguishedName distinguishedName, RSA rsa)
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
        public X509Certificate2 LoadPrivateKey()
        {
            if (_certificate != null)
                return _certificate;

            // double locking 
            lock ( _syncObject )
            {
                if (_certificate != null)
                    return _certificate;

                if (!Exists())
                    throw new CryptographicException("PrivateKey does not exist");

                _certificate = new X509Certificate2(PrivateKeyFileName, VeryBadNeverUseCertificatePassword);
                return _certificate;
            }
        }

        /// <inheritdoc/>
        public void AttachCertificate(X509Certificate2 cert)
        {
            // heavily modified version of:
            // https://stackoverflow.com/questions/18462064/associate-a-private-key-with-the-x509certificate2-class-in-net

            // we can't use LoadPrivateKey here as it creating non-exportable key
            var pk = new X509Certificate2(PrivateKeyFileName, VeryBadNeverUseCertificatePassword, X509KeyStorageFlags.Exportable);
            using var rsa = pk.GetRSAPrivateKey();
            if (rsa == null)
                throw new CryptographicException("Can't get PrivateKey");

            var newPk = cert.CopyWithPrivateKey(rsa);

            var pkcs12data = newPk.Export(X509ContentType.Pfx, VeryBadNeverUseCertificatePassword);
            File.WriteAllBytes(PrivateKeyFileName, pkcs12data);

            lock ( _syncObject )
            {
                _certificate?.Dispose();
                _certificate = null;
                // it will be loaded on the next access
            }
        }

    }
}
