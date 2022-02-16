using QRBee.Core.Security;

namespace QRBee.Api.Services
{
    /// <summary>
    /// Private key handler for API server
    /// </summary>
    public class ServerPrivateKeyHandler : PrivateKeyHandlerBase
    {
        private const string CACommonName = "QRBee-CA";
        
        private const string VeryBadAndInsecureCertificatePassword = "U…Š)+œ¶€=ø‘ c¬Í↨ð´áY/ÿ☼æX";

        public ServerPrivateKeyHandler()
        {
            CertificatePassword = VeryBadAndInsecureCertificatePassword;
            CommonName = CACommonName;
        }

    }
}
