using System.Security.Cryptography.X509Certificates;

namespace QRBee.Core.Security
{
    /// <summary>
    /// All cryptographic primitives are here.
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// Private key handler
        /// </summary>
        /// <returns></returns>
        IPrivateKeyHandler PrivateKeyHandler { get; }

        // -------------------------- encryption --------------------------

        /// <summary>
        /// Sign block of data
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <returns>Signature</returns>
        /// <exception cref="CryptographicException"></exception>
        byte[] Sign(byte [] data);

        /// <summary>
        /// Verify digital signature
        /// </summary>
        /// <param name="data">Source data</param>
        /// <param name="signature">Signature to check</param>
        /// <param name="signedBy">Public key certificate to use</param>
        /// <returns></returns>
        /// <exception cref="CryptographicException"></exception>
        bool Verify(byte [] data, byte [] signature, X509Certificate2 signedBy);

        /// <summary>
        /// Encrypt data for the selected client identified by X.509 certificate.
        /// </summary>
        /// <param name="data">Clear data to encrypt</param>
        /// <param name="destCert">Certificate of the destination client</param>
        /// <returns>Encrypted data</returns>
        /// <exception cref="CryptographicException"></exception>
        byte[] Encrypt(byte[] data, X509Certificate2 destCert);

        /// <summary>
        /// Decrypt data encrypted for this service
        /// </summary>
        /// <param name="data">Binary encrypted data</param>
        /// <returns>Decrypted data</returns>
        /// <exception cref="CryptographicException"></exception>
        byte[] Decrypt(byte[] data);


        // -------------------------- certificate services --------------------------

        /// <summary>
        /// API Server certificate
        /// </summary>
        X509Certificate2 APIServerCertificate { get; set; }

        /// <summary>
        /// Convert binary block to X509Certificate2.
        /// <see cref="X509Certificate2.CreateFromPem"/>
        /// </summary>
        /// <param name="pemData">PEM-encoded certificate</param>
        /// <returns></returns>
        X509Certificate2 Deserialize(string pemData);

        /// <summary>
        /// Convert certificate to PEM-encoded string.
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        string Serialize(X509Certificate2 cert);

        /// <summary>
        /// Get certificate serial number.
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        string GetSerialNumber(X509Certificate2 cert);

        /// <summary>
        /// Check if certificate is valid for this particular service.
        /// Note that such certificates will (and should) fail normal cert chain check.
        /// Valid certificates issued by different authority will fail the test.
        /// </summary>
        /// <param name="destCert">Certificate to check</param>
        /// <returns>True is certificate is valid for this service use</returns>
        bool IsValid(X509Certificate2 destCert);

        /// <summary>
        /// Issue client certificate
        /// </summary>
        /// <param name="subjectName">Client name (goes to CN=)</param>
        /// <param name="rsaPublicKey">Client's RSA public key</param>
        /// <returns>Certificate</returns>
        X509Certificate2 CreateCertificate(string subjectName, byte[] rsaPublicKey);
    }
}
