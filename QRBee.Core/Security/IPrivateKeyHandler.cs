using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace QRBee.Core.Security
{
    /// <summary>
    /// Private key manipulation methods
    /// </summary>
    public interface IPrivateKeyHandler
    {
        /// <summary>
        /// Check if private key exists on this machine
        /// </summary>
        /// <returns></returns>
        bool Exists();

        /// <summary>
        /// Generate new private key and store it
        /// </summary>
        /// <param name="subjectName"></param>
        /// <returns>Certificate request to be sent to CA in PEM format</returns>
        ReadableCertificateRequest GeneratePrivateKey(string subjectName = null);

        /// <summary>
        /// Re-create certificate request if CA response was not received in time.
        /// </summary>
        /// <returns>Certificate request to be sent to CA in PEM format</returns>
        ReadableCertificateRequest CreateCertificateRequest(string subjectName);

        /// <summary>
        /// Attach CA-generated public key certificate to the private key
        /// and store it
        /// </summary>
        /// <param name="cert"></param>
        void AttachCertificate(X509Certificate2 cert);

        /// <summary>
        /// Load private key. Note that public key certificate part can be
        /// self-signed until CA issues a proper certificate
        /// </summary>
        /// <returns></returns>
        RSA LoadPrivateKey();

        /// <summary>
        /// Get public key certificate
        /// </summary>
        /// <returns>Public key certificate</returns>
        X509Certificate2 GetCertificate();
    }
}
