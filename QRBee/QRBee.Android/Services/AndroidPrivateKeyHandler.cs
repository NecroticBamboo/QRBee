﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Javax.Xml.Transform;
using QRBee.Core.Security;

namespace QRBee.Droid.Services
{
    /// <summary>
    /// Private key handler for API server
    /// </summary>
    public class AndroidPrivateKeyHandler : IPrivateKeyHandler
    {
        private X509Certificate2? _certificate;
        private readonly object   _syncObject = new object();

        private const string FileName             = "private_key.p12";
        protected string CommonName { get; set; }
        private const int RSABits                 = 2048;
        private const int CertificateValidityDays = 3650;
      
        protected string CertificatePassword { get; set; }

        private string PrivateKeyFileName => $"{System.Environment.SpecialFolder.LocalApplicationData}/{FileName}";

        /// <inheritdoc/>
        public bool Exists()
            => File.Exists(PrivateKeyFileName);

        /// <inheritdoc/>
        public ReadableCertificateRequest GeneratePrivateKey(string? subjectName)
        {
            // locking used to make sure that only one thread generating a private key
            lock (_syncObject)
            {
                var pk = CreateSelfSignedClientCertificate(subjectName ?? CommonName);
                var pkcs12data = pk.Export(X509ContentType.Pfx, CertificatePassword);
                File.WriteAllBytes(PrivateKeyFileName, pkcs12data);

                _certificate?.Dispose();
                _certificate = new X509Certificate2(pkcs12data, CertificatePassword);
            }

            return CreateCertificateRequest();
        }

        /// <inheritdoc/>
        public ReadableCertificateRequest CreateCertificateRequest()
        {
            var pk = LoadPrivateKey();
            var rsa = pk.GetRSAPublicKey();

            var request = new ReadableCertificateRequest
            {
                RsaPublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
                SubjectName = pk.SubjectName.Name
            };
            var data = Encoding.UTF8.GetBytes(request.AsDataForSignature());

            //We can't use SecurityService here because it uses this class. This creates cyclic dependency.
            var signature = rsa?.SignData(data, 0, data.Length, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1) ?? throw new CryptographicException("No private key found");
            request.Signature = Convert.ToBase64String(signature);
            return request;
        }

        /// <summary>
        /// Generate EXPORTABLE certificate
        /// </summary>
        /// <param name="subjectName"></param>
        /// <returns></returns>
        private X509Certificate2 CreateSelfSignedClientCertificate(string subjectName)
        {
            // https://stackoverflow.com/questions/42786986/how-to-create-a-valid-self-signed-x509certificate2-programmatically-not-loadin

            var distinguishedName = new X500DistinguishedName($"CN={subjectName}");

            using var rsa = RSA.Create(RSABits);
            var request = CreateRequest(distinguishedName, rsa);

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
        private static CertificateRequest CreateRequest(X500DistinguishedName distinguishedName, RSA rsa)
        {
            //TODO not supported on Android
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

                _certificate = new X509Certificate2(PrivateKeyFileName, CertificatePassword);
                return _certificate;
            }
        }

        /// <inheritdoc/>
        public void AttachCertificate(X509Certificate2 cert)
        {
            // heavily modified version of:
            // https://stackoverflow.com/questions/18462064/associate-a-private-key-with-the-x509certificate2-class-in-net

            // we can't use LoadPrivateKey here as it creating non-exportable key
            var pk = new X509Certificate2(PrivateKeyFileName, CertificatePassword, X509KeyStorageFlags.Exportable);
            using var rsa = pk.GetRSAPrivateKey();
            if (rsa == null)
                throw new CryptographicException("Can't get PrivateKey");

            var newPk = cert.CopyWithPrivateKey(rsa);

            var pkcs12data = newPk.Export(X509ContentType.Pfx, CertificatePassword);
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