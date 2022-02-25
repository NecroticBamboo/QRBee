using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace QRBee.Core.Security
{

    public abstract class SecurityServiceBase : ISecurityService
    {
        protected IPrivateKeyHandler PrivateKeyHandler { get; }

        protected SecurityServiceBase(IPrivateKeyHandler privateKeyHandler)
        {
            PrivateKeyHandler = privateKeyHandler;
        }


        /// <inheritdoc/>
        public abstract X509Certificate2 CreateCertificate(string subjectName, byte[] rsaPublicKey);
        /// <inheritdoc/>
        public abstract X509Certificate2 Deserialize(string pemData);
        /// <inheritdoc/>
        public abstract string Serialize(X509Certificate2 cert);

        /// <summary>
        /// Subject may only contain letters, numbers, -, . and spaces.
        /// Subject should not be an empty string.
        /// International letters are supported, but not tested.
        /// </summary>
        /// <param name="subjectName"></param>
        /// <returns>True for valid subject name</returns>
        protected static bool IsValidSubjectName(string subjectName)
        {
            if (string.IsNullOrWhiteSpace(subjectName))
                return false;
            return Regex.IsMatch(@"[\w\s[0-9]\-\.]+", subjectName);
        }

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] data)
        {
            using var rsa = LoadPrivateKey();
            var res = rsa?.Decrypt(data, RSAEncryptionPadding.Pkcs1) ?? throw new CryptographicException("No private key found");
            return res;
        }

        public abstract X509Certificate2 APIServerCertificate { get; set; }

        /// <inheritdoc/>
        public byte [] Encrypt(byte[] data, X509Certificate2 destCert)
        {
            using var rsa = destCert.GetRSAPublicKey();
            var res = rsa?.Encrypt(data, RSAEncryptionPadding.Pkcs1) ?? throw new CryptographicException("No destination public key found");
            return res;
        }

        /// <inheritdoc/>
        public bool IsValid(X509Certificate2 destCert)
        {
            // check if certificate issued by this service
            return true;
        }

        /// <inheritdoc/>
        public byte[] Sign(byte[] data)
        {
            using var rsa = LoadPrivateKey();
            var res = rsa?.SignData(data, 0, data.Length, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ?? throw new CryptographicException("No private key found");
            return res;
        }

        /// <inheritdoc/>
        public bool Verify(byte[] data, byte[] signature, X509Certificate2 signedBy)
        {
            if ( !IsValid(signedBy))
                throw new CryptographicException("Signer's certificate is not valid");
            using var rsa = signedBy.GetRSAPublicKey();
            var res = rsa?.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ?? throw new CryptographicException("No signer public key found");
            return res;
        }

        /// <inheritdoc/>
        public string GetSerialNumber(X509Certificate2 cert)
        {
            var serNo = cert.SerialNumber;
            return serNo;
        }

        private RSA LoadPrivateKey()
        {
            if (!PrivateKeyHandler.Exists())
                PrivateKeyHandler.GeneratePrivateKey(); //TODO: subject name

            var pk = PrivateKeyHandler.LoadPrivateKey();

            return pk;
        }

    }
}
