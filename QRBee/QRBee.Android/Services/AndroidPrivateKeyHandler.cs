using QRBee.Core.Security;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace QRBee.Droid.Services
{
    /// <summary>
    /// Private key handler for API server
    /// </summary>
    public class AndroidPrivateKeyHandler : IPrivateKeyHandler
    {
        private X509Certificate2 _certificate;
        private readonly object   _syncObject = new object();

        private const string RawRsaKeyFileName = "rsa.key";
        private const string SignedCertificateFileName = "private_key.p12";
        private const string VeryBadNeverUsePrivateKeyPassword = "’³¶¾]Ô<N◄¾♪¢ :6TyŽ÷ç♦Mô¶–²ùPÎJj";
        private const int EncryptionIterationCount = 534;

        private const int RSABits                 = 2048;
        private const int CertificateValidityDays = 3650;
      
        private string PrivateKeyFileName => $"{Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}/{SignedCertificateFileName}";
        private string PrivateRsaKeyFileName => $"{Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}/{RawRsaKeyFileName}";

        /// <inheritdoc/>
        public bool Exists()
            => File.Exists(PrivateKeyFileName);

        /// <inheritdoc/>
        public ReadableCertificateRequest GeneratePrivateKey(string subjectName)
        {
            // locking used to make sure that only one thread generating a private key
            lock (_syncObject)
            {
                if ( File.Exists(PrivateRsaKeyFileName) )
                    File.Delete(PrivateRsaKeyFileName);

                using var rsa = RSA.Create(RSABits);

                var s = ExportKeyToJson(rsa,true);

                var bytes = CryptoHelper.EncryptStringAES(s, VeryBadNeverUsePrivateKeyPassword);
                File.WriteAllBytes(PrivateRsaKeyFileName, bytes);
            }

            return CreateCertificateRequest(subjectName);
        }

        private static string ExportKeyToJson(RSA rsa,bool includePrivateKey)
        {
            //Workaround for absence of half of cryptography subsystem in Mono
            var stringParameters = ExportKey(rsa, includePrivateKey);
            var s = stringParameters.ConvertToJson();
            return s;
        }

        private static StringRSAParameters ExportKey(RSA rsa, bool includePrivateKey)
        {
            var rsaParameters = rsa.ExportParameters(includePrivateKey);
            var stringParameters = new StringRSAParameters
            {
                StringExponent = SafeConvertToBase64(rsaParameters.Exponent),
                StringModulus  = SafeConvertToBase64(rsaParameters.Modulus),
                StringP        = SafeConvertToBase64(rsaParameters.P),
                StringQ        = SafeConvertToBase64(rsaParameters.Q),
                StringDP       = SafeConvertToBase64(rsaParameters.DP),
                StringDQ       = SafeConvertToBase64(rsaParameters.DQ),
                StringInverseQ = SafeConvertToBase64(rsaParameters.InverseQ),
                StringD        = SafeConvertToBase64(rsaParameters.D)
            };
            return stringParameters;
        }
        
        private static string SafeConvertToBase64(byte[] bytes) => bytes == null ? "" : Convert.ToBase64String(bytes);

        /// <inheritdoc/>
        public ReadableCertificateRequest CreateCertificateRequest(string subjectName)
        {
            using var rsa = LoadPrivateKey();

            var request = new ReadableCertificateRequest
            {
                RsaPublicKey = ExportKey(rsa,false),
                SubjectName = subjectName
            };
            var data = Encoding.UTF8.GetBytes(request.AsDataForSignature());

            //We can't use SecurityService here because it uses this class. This creates cyclic dependency.
            var signature = rsa?.SignData(data, 0, data.Length, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ?? throw new CryptographicException("No private key found");
            request.Signature = Convert.ToBase64String(signature);
            return request;
        }

        ///// <summary>
        ///// Generate EXPORTABLE certificate
        ///// </summary>
        ///// <param name="subjectName"></param>
        ///// <returns></returns>
        //private X509Certificate2 CreateSelfSignedClientCertificate(string subjectName)
        //{
        //    // https://stackoverflow.com/questions/42786986/how-to-create-a-valid-self-signed-x509certificate2-programmatically-not-loadin

        //    var distinguishedName = new X500DistinguishedName($"CN={subjectName}");

        //    using var rsa = RSA.Create(RSABits);
        //    var request = CreateRequest(distinguishedName, rsa);

        //    var certificate = request.CreateSelfSigned(
        //        new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
        //        new DateTimeOffset(DateTime.UtcNow.AddDays(CertificateValidityDays))
        //        );

        //    return certificate;
        //}

        ///// <summary>
        ///// Generate CA certificate request (i.e. with KeyCertSign usage extension)
        ///// </summary>
        ///// <param name="distinguishedName"></param>
        ///// <param name="rsa"></param>
        ///// <returns></returns>
        //private static CertificateRequest CreateRequest(X500DistinguishedName distinguishedName, RSA rsa)
        //{
        //    //TODO not supported on Android
        //    var request = new CertificateRequest(
        //        distinguishedName,
        //        rsa,
        //        HashAlgorithmName.SHA256,
        //        RSASignaturePadding.Pkcs1
        //        );

        //    request.CertificateExtensions.Add(
        //        new X509KeyUsageExtension(
        //              X509KeyUsageFlags.DataEncipherment
        //            | X509KeyUsageFlags.KeyEncipherment
        //            | X509KeyUsageFlags.DigitalSignature,
        //    false));

        //    return request;
        //}


        public X509Certificate2 GetCertificate()
        {
            
            var cert = new X509Certificate2(PrivateKeyFileName);
            return cert;
        }

        /// <inheritdoc/>
        public void AttachCertificate(X509Certificate2 cert)
        {
            var bytes = cert.Export(X509ContentType.Cert);

            File.WriteAllBytes(PrivateKeyFileName, bytes);

            lock ( _syncObject )
            {
                _certificate?.Dispose();
                _certificate = null;
                // it will be loaded on the next access
            }
        }

        public RSA LoadPrivateKey()
        {
            var bytes = File.ReadAllBytes(PrivateRsaKeyFileName);
            var s = CryptoHelper.DecryptStringAES(bytes, VeryBadNeverUsePrivateKeyPassword);

            var stringParameters = StringRSAParameters.ConvertFromJson(s);
            var rsaParameters = new RSAParameters
            {
                Exponent = SafeConvertFromBase64(stringParameters.StringExponent),
                Modulus  = SafeConvertFromBase64(stringParameters.StringModulus),
                P        = SafeConvertFromBase64(stringParameters.StringP),
                Q        = SafeConvertFromBase64(stringParameters.StringQ),
                DP       = SafeConvertFromBase64(stringParameters.StringDP),
                DQ       = SafeConvertFromBase64(stringParameters.StringDQ),
                InverseQ = SafeConvertFromBase64(stringParameters.StringInverseQ),
                D        = SafeConvertFromBase64(stringParameters.StringD)
            };

            var rsa = RSA.Create(rsaParameters);
            if (rsa == null)
                throw new CryptographicException("Can't get PrivateKey");
            return rsa;
        }

        private static byte[] SafeConvertFromBase64(string s) => string.IsNullOrWhiteSpace(s) ? null : Convert.FromBase64String(s);
    }
}
